// © 2019 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Collections.ObjectModel;
using CommerceOps.Sitecore.Commerce.Core;
using FluentAssertions;
using Sitecore.Commerce.Extensions;
using Sitecore.Commerce.Sample.Contexts;
using Sitecore.Commerce.ServiceProxy;

namespace Sitecore.Commerce.Sample.Console.Extensions
{
    public static class Minions
    {
        public static void RunMinion(string minionFullName, string environmentName)
        {
            var policies = new Collection<Policy>
            {
                new RunMinionPolicy
                {
                    RunChildren = false
                }
            };

            var minionResult = Proxy.GetValue(new MinionRunner().Context.MinionsContainer()
                .RunMinion(minionFullName, environmentName, policies));

            minionResult.Messages.Should().NotContainErrors();
            minionResult.WaitUntilCompletion();
        }

        public static void RunFullIndexMinions(string environmentName)
        {
            RunMinion("Sitecore.Commerce.Plugin.Search.FullIndexMinion, Sitecore.Commerce.Plugin.Search", environmentName);
        }
    }
}
