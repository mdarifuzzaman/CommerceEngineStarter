using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Commerce.Plugin.Search.Solr;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using SolrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sample.Order.Pipelines.EntityViewBlocks
{
    [PipelineDisplayName("Search.Solr.Block.ProcessExtendedDocumentSearchResult")]
    public class ProcessExtendedDocumentSearchResultBlock : AsyncPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly IGetEntityViewPipeline _getEntityViewPipeline;

        public ProcessExtendedDocumentSearchResultBlock(IGetEntityViewPipeline getEntityViewPipeline)
     : base()
        {
            this._getEntityViewPipeline = getEntityViewPipeline;
        }

        //public ProcessExtendedDocumentSearchResultBlock() => this.BlockCondition = new Predicate<IPipelineExecutionContext>(ValidatePolicy);

        //public override EntityView ContinueTask(
        //  EntityView arg,
        //  CommercePipelineExecutionContext context)
        //{
        //    return arg;
        //}

        public override async Task<EntityView> RunAsync(
          EntityView arg,
          CommercePipelineExecutionContext context)
        {
            Condition.Requires<EntityView>(arg).IsNotNull<EntityView>(this.Name + ": argument can not be null.");
            var requests = context.CommerceContext.GetObjects<EntityViewArgument>();

            //var extendedEntity = await _getEntityViewPipeline.RunAsync(new EntityViewArgument() { EntityId = "ExtendedSearch_001", ForAction= "ExtendedSearch" }, context);
            
            
            var extendedObject = context.CommerceContext.GetObject<ExtendedSearchModel>();
            SolrQueryResults<Document> solrQueryResults = extendedObject.ExtendedDocument;

            if (extendedObject.ExtendedDocument.Count <= 0) return arg;

            EntityView entityView1 = new EntityView();
            entityView1.Name = "ExtendedSearchResult";
            entityView1.UiHint = "Table";
            entityView1.EntityId = "ExtendedSearchView";

            if (solrQueryResults == null || !solrQueryResults.Any<Document>())
                return entityView1;

            if (solrQueryResults.NumFound > 0)
            {
                List<ViewProperty> properties = entityView1.Properties;
                ViewProperty viewProperty = new ViewProperty();
                viewProperty.Name = "Count";
                viewProperty.IsRequired = false;
                viewProperty.IsHidden = true;
                viewProperty.IsReadOnly = true;
                viewProperty.RawValue = (object)solrQueryResults.NumFound;
                properties.Add(viewProperty);
            }

            List<Document> documents = new List<Document>(solrQueryResults.Count);

            solrQueryResults.ForEach((Action<Document>)(r =>
            {
                Document document = new Document();
                foreach (string key in r.Keys)
                    document.Add(key, this.ConvertKeyValue(r[key]));
                documents.Add(document);
                EntityView entityView = new EntityView();
                entityView.EntityId = document["productid_t"].ToString();
                entityView.UiHint = "";
                arg.ChildViews.Add(entityView);
                
            }));

            context.CommerceContext.RemoveObject(extendedObject);
            context.CommerceContext.AddObject(new ExtendedDocumentSearchModel() { Documents = documents });

            arg.ChildViews.Add(entityView1);

            return await Task.FromResult(arg);
        }

        protected virtual object ConvertKeyValue(object o)
        {
            object obj = o;
            if (obj is DateTime)
                obj = (object)((DateTime)obj).ConvertDateTimeToDateTimeOffset();
            return obj;
        }

        private static bool ValidatePolicy(IPipelineExecutionContext obj) => ((CommercePipelineExecutionContext)obj).CommerceContext.HasPolicy<SolrSearchPolicy>();
    }
}
