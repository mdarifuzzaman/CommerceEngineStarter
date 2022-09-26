// © 2019 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Sample.Console;

namespace Sitecore.Commerce.Sample.Contexts
{
    public class GlobalDbRunner
    {
        public GlobalDbRunner()
        {
            Context = new ShopperContext
            {
                Environment = null,
                Shop = Program.DefaultStorefront,
                ShopperId = "GlobalDbRunnerShopperId",
                Language = "en-US",
                Currency = "USD",
                PolicyKeys = "ZeroMinionDelay|xActivityPerf",
                EffectiveDate = DateTimeOffset.Now,
                Components = new List<Component>()
            };
        }

        public ShopperContext Context { get; set; }
    }
}
