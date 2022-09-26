// © 2019 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Linq;
using System.Threading.Tasks;
using CommerceOps.Sitecore.Commerce.Core;
using FluentAssertions;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.Extensions;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Commerce.Sample.Console.Extensions;
using Sitecore.Commerce.Sample.Contexts;
using SearchScopePolicy = CommerceOps.Sitecore.Commerce.Plugin.Search.SearchScopePolicy;

namespace Sitecore.Commerce.Sample.Console
{
    public static class GenericSearch
    {
        private static readonly Container ShopsContainer = new CsrSheila().Context.ShopsContainer();
        private static readonly CommerceOps.Sitecore.Commerce.Engine.Container OpsContainer = new GlobalDbRunner().Context.OpsContainer();

        public static async Task RunScenarios()
        {
            using (new SampleScenarioScope(nameof(GenericSearch)))
            {
                Minions.RunFullIndexMinions(Constants.AWMinionsEnvironmentName);

                var policySet = OpsContainer.PolicySets.ByKey("SearchPolicySet").GetValue();
                var scopeName = policySet.Policies.OfType<SearchScopePolicy>()
                    .First(p => p.Name.Contains("CatalogItemsScope")).Name;

                var searchQuery = new SearchQuery
                {
                    SearchNode = new MatchAllSearchNode()
                };

                var filterQuery = new FilterQuery
                {
                    FilterNode = new ContainsFilterNode
                    {
                        FieldName = new FieldNameFilterNode
                        {
                            Name = "name"
                        },
                        FieldValue = new FieldValueFilterNode
                        {
                            Value = "Diamond",
                            ValueType = FilterNodeValueType.Text
                        }
                    }
                };

                var searchOptions = new SearchOptions
                {
                    Top = 3
                };

                var results = await ShopsContainer.GenericSearch(scopeName, searchQuery,
                    filterQuery, searchOptions).ExecuteAsync().ConfigureAwait(false);

                results.Should().HaveCount(searchOptions.Top);
            }
        }
    }
}
