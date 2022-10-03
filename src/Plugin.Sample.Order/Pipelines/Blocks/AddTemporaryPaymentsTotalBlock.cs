// © 2017 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.Order
{
    [PipelineDisplayName("Orders.block.Sample.AddTemporaryPaymentTotal")]
    public class AddTemporaryPaymentsTotalBlock : AsyncPipelineBlock<CartEmailArgument, CartEmailArgument, CommercePipelineExecutionContext>
    {

     public override async Task<CartEmailArgument> RunAsync(
     CartEmailArgument arg,
     CommercePipelineExecutionContext context)
        {
            // AddTemporaryPaymentsTotal
            arg.Cart.Totals.PaymentsTotal = arg.Cart.Totals.GrandTotal;
            return await Task.FromResult(arg);
        }
    }
}
