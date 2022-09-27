// © 2015 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
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
                        .ConfigurePipeline<ICreateOrderPipeline>(
                            d =>
                            {
                                d.Replace<Sitecore.Commerce.Plugin.Orders.CreateOrderBlock, CreateOrderBlock>();
                            }));
        }
    }
}
