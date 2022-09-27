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
    [PipelineDisplayName("Orders.block.Sample.CreateOrder")]
    public class CreateOrderBlock : AsyncPipelineBlock<CartEmailArgument, Sitecore.Commerce.Plugin.Orders.Order, CommercePipelineExecutionContext>
    {

     public override async Task<Sitecore.Commerce.Plugin.Orders.Order> RunAsync(
     CartEmailArgument arg,
     CommercePipelineExecutionContext context)
        {
            CreateOrderBlock createOrderBlock = this;
            // ISSUE: explicit non-virtual call
            Condition.Requires<CartEmailArgument>(arg).IsNotNull<CartEmailArgument>(createOrderBlock.Name + ": arg can not be null");
            // ISSUE: explicit non-virtual call
            Condition.Requires<Cart>(arg.Cart).IsNotNull<Cart>(createOrderBlock.Name + ": The cart can not be null");
            CommercePipelineExecutionContext executionContext;
            if (string.IsNullOrEmpty(arg.Email))
            {
                executionContext = context;
                executionContext.Abort(await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "EmailIsRequired", new object[1], "Can not create order, email address is required.").ConfigureAwait(false), (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (Sitecore.Commerce.Plugin.Orders.Order)null;
            }
            Cart cart = arg.Cart;
            if (!cart.Lines.Any<CartLineComponent>())
            {
                executionContext = context;
                CommerceContext commerceContext = context.CommerceContext;
                string error = context.GetPolicy<KnownResultCodes>().Error;
                object[] args = new object[1] { (object)cart.Id };
                string defaultMessage = "Can not create order, cart " + cart.Id + " has no lines";
                executionContext.Abort(await commerceContext.AddMessage(error, "OrderHasNoLines", args, defaultMessage).ConfigureAwait(false), (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (Sitecore.Commerce.Plugin.Orders.Order)null;
            }
            foreach (Component component in cart.Lines.Where<CartLineComponent>((Func<CartLineComponent, bool>)(l => l.HasPolicy<PurchaseOptionMoneyPolicy>())))
                component.GetPolicy<PurchaseOptionMoneyPolicy>().FixedSellPrice = false;
            context.CommerceContext.AddModel((Model)cart.Totals);
            context.CommerceContext.AddUniqueEntity((CommerceEntity)cart);
            if (Decimal.Compare(cart.Totals.PaymentsTotal.Amount, cart.Totals.GrandTotal.Amount) != 0)
            {
                executionContext = context;
                executionContext.Abort(await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "InsufficientPayment", (object[])null, "Order Payments Total must equal the GrandTotal").ConfigureAwait(false), (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (Sitecore.Commerce.Plugin.Orders.Order)null;
            }
            ContactComponent component1 = cart.GetComponent<ContactComponent>();
            component1.Email = arg.Email;
            KnownOrderListsPolicy policy1 = context.GetPolicy<KnownOrderListsPolicy>();
            GlobalOrderPolicy policy2 = context.GetPolicy<GlobalOrderPolicy>();
            string str1 = string.Format("{0}{1:N}", (object)CommerceEntity.IdPrefix<Sitecore.Commerce.Plugin.Orders.Order>(), (object)Guid.NewGuid());
            Sitecore.Commerce.Plugin.Orders.Order order1 = new Sitecore.Commerce.Plugin.Orders.Order();
            order1.Id = str1;
            order1.Name = cart.Name;
            order1.DisplayName = cart.DisplayName;
            order1.Totals = cart.Totals;
            order1.Lines = cart.Lines;
            order1.Adjustments = cart.Adjustments;
            order1.ShopName = cart.ShopName;
            order1.FriendlyId = str1;
            order1.OrderConfirmationId = string.Empty;
            order1.OrderPlacedDate = DateTimeOffset.UtcNow;
            order1.Status = policy2.CreatedOrderStatus;
            Sitecore.Commerce.Plugin.Orders.Order order2 = order1;
            order2.AddComponents(cart.EntityComponents.ToArray<Component>());
            order2.AddPolicies(cart.EntityPolicies.ToArray<Policy>());
            string str2 = component1.IsRegistered ? policy1.AuthenticatedOrders : policy1.AnonymousOrders;
            ListMembershipsComponent membershipsComponent1 = new ListMembershipsComponent()
            {
                Memberships = (IList<string>)new List<string>()
        {
          CommerceEntity.ListName<Sitecore.Commerce.Plugin.Orders.Order>(),
          str2
        }
            };
            if (component1.IsRegistered && !string.IsNullOrEmpty(component1.CustomerId))
                membershipsComponent1.Memberships.Add(string.Format((IFormatProvider)CultureInfo.InvariantCulture, context.GetPolicy<KnownOrderListsPolicy>().CustomerOrders, (object)component1.CustomerId));
            order2.SetComponent((Component)membershipsComponent1);
            Sitecore.Commerce.Plugin.Orders.Order order3 = order2;
            TransientListMembershipsComponent membershipsComponent2 = new TransientListMembershipsComponent();
            membershipsComponent2.Memberships = (IList<string>)new List<string>()
      {
        policy1.PendingOrders
      };
            order3.SetComponent((Component)membershipsComponent2);
            context.Logger.LogInformation(string.Format("Orders.CreateOrder: OrderId={0}|GrandTotal={1} {2}", (object)str1, (object)order2.Totals.GrandTotal.CurrencyCode, (object)order2.Totals.GrandTotal.Amount));
            context.CommerceContext.AddModel((Model)new CreatedOrder()
            {
                OrderId = str1
            });
            return order2;
        }

        public CreateOrderBlock()
          : base()
        {
        }
    }
}
