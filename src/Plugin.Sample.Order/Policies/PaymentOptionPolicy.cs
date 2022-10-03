using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Sample.Order.Policies
{
    public class PaymentOptionPolicy: Policy
    {
        public bool RequiredPaymentCheck { get; set; }
    }
}
