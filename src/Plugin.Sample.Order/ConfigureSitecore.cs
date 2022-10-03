// © 2015 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Sample.Order.Pipelines.Blocks;
using Plugin.Sample.Order.Pipelines.EntityViewBlocks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Plugin.Sample.Order
{
    /// <summary>
    /// The Habitat configure class.
    /// </summary>
    /// <seealso cref="IConfigureSitecore" />
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(
                config =>
                    config
                        .ConfigurePipeline<IGetEntityViewPipeline>(pipeline => pipeline
                            .Add<GetSearchExtensionViewBlock>().After<PopulateEntityVersionBlock>()
                        )
                        .ConfigurePipeline<ISearchPipeline>(d =>
                        {
                            d.Replace<Sitecore.Commerce.Plugin.Search.Solr.QueryDocumentsBlock, QueryDocumentsBlock>();
                        })
                        .ConfigurePipeline<ISearchPipeline>(d =>
                        {
                            d.Add<ProcessExtendedDocumentSearchResultBlock>().After<Sitecore.Commerce.Plugin.Pricing.ProcessDocumentSearchResultBlock>();
                            d.Add<ProcessExtendedCatalogDocumentSearchResultBlock>().After<ProcessExtendedDocumentSearchResultBlock>();
                        })
                        .ConfigurePipeline<ICreateOrderPipeline>(
                            d =>
                            {
                                d.Replace<Sitecore.Commerce.Plugin.Orders.CreateOrderBlock, CreateOrderBlock>();
                            }));
        }
    }
}
