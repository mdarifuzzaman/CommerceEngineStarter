using HttpWebAdapters;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Commerce.Plugin.Search.Solr;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using SolrNet;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sample.Order.Pipelines.Blocks
{
    [PipelineDisplayName("Search.Solr.Block.QueryDocuments")]
    public class QueryDocumentsBlock : AsyncConditionalPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly SolrContextCommand _searchCommand;
        private readonly IFilterNodeVisitorFactory _factory;
        private IGetSolrAuthProviderPipeline _solrAuthPipeline;

        public QueryDocumentsBlock(
          SolrContextCommand solrContextCommand,
          IGetSolrAuthProviderPipeline solrAuthPipeline,
          IFilterNodeVisitorFactory factory)
        {
            this._factory = Condition.Requires<IFilterNodeVisitorFactory>(factory, nameof(factory)).IsNotNull<IFilterNodeVisitorFactory>().Value;
            this._searchCommand = solrContextCommand;
            this.BlockCondition = new Predicate<IPipelineExecutionContext>(QueryDocumentsBlock.ValidatePolicy);
            this._solrAuthPipeline = solrAuthPipeline;
        }

        public override Task<EntityView> ContinueTask(
          EntityView arg,
          CommercePipelineExecutionContext context)
        {
            return Task.FromResult<EntityView>(arg);
        }

        public override async Task<EntityView> RunAsync(
          EntityView arg,
          CommercePipelineExecutionContext context)
        {
            QueryDocumentsBlock queryDocumentsBlock = this;
            // ISSUE: explicit non-virtual call
            Condition.Requires<EntityView>(arg).IsNotNull<EntityView>(queryDocumentsBlock.Name + ": The argument cannot be null.");
            CommercePipelineExecutionContext executionContext;
            if (!arg.HasPolicy<SearchScopePolicy>())
            {
                executionContext = context;
                executionContext.Abort(await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "DocumentsViewError", (object[])null, "Can not perform the query. Documents view is missing information.").ConfigureAwait(false), (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (EntityView)null;
            }
            SearchScopePolicy policy = arg.GetPolicy<SearchScopePolicy>();
            if (string.IsNullOrEmpty(policy.Name))
            {
                executionContext = context;
                executionContext.Abort(await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "DocumentsViewError", (object[])null, "Can not perform the query. Documents view is missing information.").ConfigureAwait(false), (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (EntityView)null;
            }
            context.CommerceContext.AddObject((object)arg);
            string search = arg.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.Equals("Term", StringComparison.OrdinalIgnoreCase)))?.Value;
            if (string.IsNullOrEmpty(search))
                search = "*";
            QueryOptions queryOptions = new QueryOptions()
            {
                Stats = new StatsParameters()
            };
            string query = arg.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.Equals("Filter", StringComparison.OrdinalIgnoreCase)))?.Value;
            if (!string.IsNullOrEmpty(query))
            {
                SolrQuery solrQuery = new SolrQuery(query);
                queryOptions.AddFilterQueries((ISolrQuery)solrQuery);
            }
            FilterQuery filterQuery = context.CommerceContext.GetObject<FilterQuery>();
            if (filterQuery != null)
            {
                IFilterNodeVisitor visitor = queryDocumentsBlock._factory.GetVisitor(context.CommerceContext);
                queryOptions.AddFilterQueries((ISolrQuery)new SolrQuery(filterQuery.GetQuery(visitor, new FilterQueryArgument())));
            }
            string orderBy = arg.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.Equals("OrderBy", StringComparison.OrdinalIgnoreCase)))?.Value;
            if (!string.IsNullOrEmpty(orderBy))
                queryOptions.OrderBy = QueryDocumentsBlock.GetSortOrderByField(orderBy);
            string s1 = arg.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.Equals("Skip", StringComparison.OrdinalIgnoreCase)))?.Value;
            int result1;
            if (!string.IsNullOrEmpty(s1) && int.TryParse(s1, out result1))
                queryOptions.StartOrCursor = (StartOrCursor)new StartOrCursor.Start(result1);
            string s2 = arg.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.Equals("Top", StringComparison.OrdinalIgnoreCase)))?.Value;
            int result2;
            if (!string.IsNullOrEmpty(s2) && int.TryParse(s2, out result2) && result2 > 0)
                queryOptions.Rows = new int?(result2);
            else
                queryOptions.Rows = new int?(context.GetPolicy<SearchPolicy>().MaxNumberOfRows);
            ICollection<string> source = context.CommerceContext.GetObject<ICollection<string>>();
            if (source != null && source.Any<string>())
                queryOptions.Fields = source;
            SolrQueryResults<Document> objectToAdd = await queryDocumentsBlock._searchCommand.QueryDocuments(policy.CurrentIndexName, search, queryOptions, context.CommerceContext).ConfigureAwait(false);
            if (objectToAdd != null)
                context.CommerceContext.AddObject((object)objectToAdd);

            //query to solr
            var policies = arg.GetPolicies<ItemIndexablePolicy>();
            var queryOptions2 = new QueryOptions()
            {
                Stats = new StatsParameters(),
                Fields = new List<string> { "product_country_t", "extended_desc_t", "extended_options_t", "productid_t", "displayname_s" }
            };

            search = "product_country_t:" + search.Split(':')[1]; //check search has any colon or not
            var masterIndex = "storefront101.sc_master_index"; //policies.SingleOrDefault(e => e.IndexName == "sitecore_master_index");            
            SolrQueryResults<Document> objectFromProductIndex = await QueryDocuments(masterIndex, search, queryOptions2, context.CommerceContext).ConfigureAwait(false);
            if (objectFromProductIndex != null)
                context.CommerceContext.AddObject(new ExtendedSearchModel() { ExtendedDocument = objectFromProductIndex});

            return arg;
        }

        protected virtual async Task<SolrQueryResults<Document>> QueryDocuments(string indexName,string search,QueryOptions queryOptions,CommerceContext context)
        {
            SolrQueryResults<Document> results = new SolrQueryResults<Document>();
            try
            {
                ISolrReadOnlyOperations<Document> ops = await this.GetSolrReadOnlyOperations(indexName, context).ConfigureAwait(false);              
                results = ops.Query(search, queryOptions);
                ops = (ISolrReadOnlyOperations<Document>)null;
            }
            catch (Exception ex)
            {
                context.LogExceptionAndMessage("SolrContextCommand.QueryDocuments", ex);
            }
            SolrQueryResults<Document> solrQueryResults = results;
            results = (SolrQueryResults<Document>)null;
            return solrQueryResults;
        }

        protected virtual async Task<ISolrReadOnlyOperations<Document>> GetSolrReadOnlyOperations(string indexName, CommerceContext context)
        {
            ISolrReadOnlyOperations<Document> readOnlyOps = (ISolrReadOnlyOperations<Document>)null;
            try
            {
                ISolrContext solrContext = await this.GetSolrSearchContext(context).ConfigureAwait(false);
                int num = await this.InitIndex(indexName, context).ConfigureAwait(false) ? 1 : 0;
                readOnlyOps = solrContext.GetSolrDocumentOperations(indexName, context);
                solrContext = (ISolrContext)null;
            }
            catch (Exception ex)
            {
                context.LogExceptionAndMessage("SolrContextCommand.GetSolrReadOnlyOperations", ex);
            }
            ISolrReadOnlyOperations<Document> readOnlyOperations = readOnlyOps;
            readOnlyOps = (ISolrReadOnlyOperations<Document>)null;
            return readOnlyOperations;
        }

        protected virtual async Task<bool> InitIndex(string indexName, CommerceContext context)
        {
            try
            {
                if (!SolrIndexManager.Exists(indexName))
                {
                    SolrConnection connection = await this.GetSolrConnection(indexName, context).ConfigureAwait(false);
                    SolrIndexManager.Init((ISolrConnection)connection, indexName);
                }
            }
            catch (Exception ex)
            {
                context.LogExceptionAndMessage("SolrContextCommand.InitIndex", ex);
            }
            finally
            {
            }
            return true;
        }

        protected virtual async Task<ISolrContext> GetSolrSearchContext(CommerceContext context)
        {
            ISolrContext solrContext = (ISolrContext)null;
            try
            {
                IHttpWebRequestFactory httpWebRequestFactory = await this.GetSolrAuth(context).ConfigureAwait(false);
                if (context.GetPolicy<SolrSearchPolicy>().IsSolrCloud)
                {                    
                    solrContext = (ISolrContext)new SolrCloudContext(httpWebRequestFactory);
                }
                else
                    solrContext = (ISolrContext)new SolrContext(httpWebRequestFactory);
            }
            catch (Exception ex)
            {
                context.LogExceptionAndMessage("SolrContextCommand.GetSolrSearchContext", ex);
            }
            ISolrContext solrSearchContext = solrContext;
            solrContext = (ISolrContext)null;
            return solrSearchContext;
        }

        protected virtual async Task<SolrConnection> GetSolrConnection(
          string indexName,
          CommerceContext context)
        {
            string url = context.GetPolicy<SolrSearchPolicy>().SolrUrl + "/" + indexName;
            int timeout = context.GetPolicy<SolrSearchPolicy>().ConnectionTimeout;
            IHttpWebRequestFactory webRequestFactory = await this.GetSolrAuth(context).ConfigureAwait(false);
            if (webRequestFactory == null)
            {
                //context.Logger.LogError("Failed to get HttpWebRequestFactory for Solr connection, please verify SolrSearchPolicy property values.");
                return (SolrConnection)null;
            }
            return new SolrConnection(url)
            {
                Timeout = timeout,
                HttpWebRequestFactory = webRequestFactory
            };
        }

        protected virtual async Task<IHttpWebRequestFactory> GetSolrAuth(CommerceContext context)
        {
            IHttpWebRequestFactory solrAuth = null;
            if (solrAuth == null)
                solrAuth = await this._solrAuthPipeline.RunAsync(new SearchArgument(), context.PipelineContext).ConfigureAwait(false);
            return solrAuth;
        }


        private static ICollection<SortOrder> GetSortOrderByField(string orderBy) => (ICollection<SortOrder>)((IEnumerable<string>)orderBy.Split(',')).Select<string, SortOrder>(new Func<string, SortOrder>(SortOrder.Parse)).Where<SortOrder>((Func<SortOrder, bool>)(parse => parse != null)).ToList<SortOrder>();

        private static bool ValidatePolicy(IPipelineExecutionContext obj) => ((CommercePipelineExecutionContext)obj).CommerceContext.HasPolicy<SolrSearchPolicy>();
    }
}
