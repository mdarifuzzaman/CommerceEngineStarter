using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sample.Order.Pipelines.EntityViewBlocks
{
    [PipelineDisplayName("Catalog.block.ProcessExtendedDocumentSearchResultBlock")]
    public class ProcessExtendedCatalogDocumentSearchResultBlock : SyncPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly IGetEntityViewPipeline _getEntityViewPipeline;
        public ProcessExtendedCatalogDocumentSearchResultBlock(IGetEntityViewPipeline getEntityViewPipeline)
     : base()
        {
            this._getEntityViewPipeline = getEntityViewPipeline;
        }
        public override EntityView Run(
          EntityView arg,
          CommercePipelineExecutionContext context)
        {
            Condition.Requires<EntityView>(arg, nameof(arg)).IsNotNull<EntityView>();
            Condition.Requires<CommercePipelineExecutionContext>(context, nameof(context)).IsNotNull<CommercePipelineExecutionContext>();
            
            var extendedObject = context.CommerceContext.GetObject<ExtendedDocumentSearchModel>();
            if (extendedObject == null || !extendedObject.Documents.Any()) return arg;

            List<Document> source = extendedObject.Documents;

            if (source == null || !source.Any<Document>() || !arg.HasPolicy<SearchScopePolicy>())
                return arg;
            SearchScopePolicy policy1 = arg.GetPolicy<SearchScopePolicy>();
            List<string> list = new List<string> { "product_country_t", "extended_desc_t", "extended_options_t", "productid_t", "displayname_s" };

            //var extendedEntityView = await _getEntityViewPipeline.RunAsync(new EntityViewArgument() { EntityId = "ExtendedSearch_001", ForAction = "ExtendedSearch" }, context);


            foreach (Document document in source)
            {
                int? entityVersion = 1;
                string docId = document["productid_t".ToLowerInvariant()].ToString();


                EntityView view = arg.ChildViews.OfType<EntityView>().FirstOrDefault<EntityView>((Func<EntityView, bool>)(c =>
                {
                    if (!c.EntityId.Equals(docId, StringComparison.OrdinalIgnoreCase))
                        return false;
                    int entityVersion1 = c.EntityVersion;
                    int? nullable = entityVersion;
                    int valueOrDefault = nullable.GetValueOrDefault();
                    return entityVersion1 == valueOrDefault & nullable.HasValue;
                }));

                if (view != null)
                {
                    ViewProperty viewProperty1 = view.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.EqualsOrdinalIgnoreCase("productid_t")));

                    if (viewProperty1 == null && list.Contains<string>("productid_t", (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase))
                    {
                        ViewProperty viewProperty2 = new ViewProperty();
                        viewProperty2.Name = "productid_t".ToLowerInvariant();
                        viewProperty2.Value = document["productid_t"].ToString();
                        viewProperty2.RawValue = document["productid_t"].ToString();
                        viewProperty2.OriginalType = typeof(string).FullName;
                        viewProperty2.IsReadOnly = true;
                        viewProperty1 = viewProperty2;
                    }
                    ViewProperty viewProperty3 = view.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.EqualsOrdinalIgnoreCase("product_country_t")));
                    if (viewProperty3 == null && list.Contains<string>("product_country_t", (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase))
                    {
                        ViewProperty viewProperty4 = new ViewProperty();
                        viewProperty4.Name = "product_country_t".ToLowerInvariant();
                        viewProperty4.Value = document["product_country_t"].ToString();
                        viewProperty4.RawValue = document["product_country_t"].ToString();
                        viewProperty4.OriginalType = typeof(string).FullName;
                        viewProperty4.IsReadOnly = true;
                        viewProperty3 = viewProperty4;
                    }
                    
                    ViewProperty viewProperty5 = view.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.EqualsOrdinalIgnoreCase("extended_options_t")));
                    if (viewProperty5 == null && list.Contains<string>("extended_options_t", (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase))
                    {
                        ViewProperty viewProperty6 = new ViewProperty();
                        viewProperty6.Name = "extended_options_t".ToLowerInvariant();
                        viewProperty6.Value = document["extended_options_t"].ToString();
                        viewProperty6.RawValue = document["extended_options_t"].ToString();
                        viewProperty6.OriginalType = typeof(string).FullName;
                        viewProperty6.IsReadOnly = true;
                        viewProperty5 = viewProperty6;
                    }

                    //ViewProperty viewProperty6 = view.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.EqualsOrdinalIgnoreCase("DisplayName")));
                    //ViewProperty viewProperty7 = view.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.EqualsOrdinalIgnoreCase("DateUpdated")));
                    //ViewProperty viewProperty8 = view.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.EqualsOrdinalIgnoreCase("DateCreated")));

                    view.Properties.Clear();
                    //if (viewProperty5 != null)
                    //    viewProperty5.UiType = "EntityLink";

                    view.AddViewPropertyToEntityViewOrDefault(viewProperty1, list, "productid_t", typeof(string).FullName);
                    view.AddViewPropertyToEntityViewOrDefault(viewProperty3, list, "product_country_t", typeof(string).FullName);
                    view.AddViewPropertyToEntityViewOrDefault(viewProperty5, list, "extended_options_t", typeof(string).FullName);
                    //view.AddViewPropertyToEntityViewOrDefault(viewProperty6, list, "DisplayName", typeof(string).FullName);                    
                    
                    //view.AddViewPropertyToEntityViewOrDefault(viewProperty8, list, "DateCreated", typeof(DateTimeOffset).FullName);
                    //view.AddViewPropertyToEntityViewOrDefault(viewProperty7, list, "DateUpdated", typeof(DateTimeOffset).FullName);
                }
            }
            return arg;
        }

        public ProcessExtendedCatalogDocumentSearchResultBlock()
          : base()
        {
        }
    }
}
