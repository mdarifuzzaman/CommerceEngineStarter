using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sample.Order.Pipelines.EntityViewBlocks
{
    public class GetSearchExtensionViewBlock : AsyncPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntityPipeline;

        public GetSearchExtensionViewBlock(IFindEntityPipeline findEntityPipeline): base()
        {
            this._findEntityPipeline = findEntityPipeline;
        }

        public override async Task<EntityView> RunAsync(EntityView entityView, CommercePipelineExecutionContext context)
        {
            // Ensure parameters are provided
            Condition.Requires(entityView, nameof(entityView)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            // Validate that the page entity is a sellable item
            // Validate the view name matches the requested page
            if (!string.IsNullOrEmpty(entityView.EntityId))
            {
                return entityView;
            }

            // Create an entity view to hold the component properties
            var entityView1 = new EntityView
            {
                DisplayName = "Extended Search Information",
                Name = "Extended Search Information",
                EntityId = "ExtendedSearch_001"
            };

            entityView1.DisplayRank = 120;
            entityView1.Action = "ExtendedSearch" ;
            entityView.ChildViews.Add(entityView1);
            List<ViewProperty> properties1 = entityView1.Properties;
            ViewProperty viewProperty1 = new ViewProperty();
            viewProperty1.Name = "Extended Result";
            viewProperty1.RawValue = (object)string.Empty;
            viewProperty1.IsRequired = false;
            properties1.Add(viewProperty1);
            return await Task.FromResult(entityView);
        }
    }
}
