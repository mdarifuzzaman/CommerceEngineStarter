using System.Linq;
using FluentAssertions;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Extensions;
using Sitecore.Commerce.Sample.Console.Extensions;
using Sitecore.Commerce.Sample.Contexts;
using Sitecore.Commerce.ServiceProxy;

namespace Sitecore.Commerce.Sample.Console
{
    public static class Search
    {
        private static readonly Container ShopsContainer = new CsrSheila().Context.ShopsContainer();

        public static void RunScenarios()
        {
            using (new SampleScenarioScope("Search"))
            {
                var view = Proxy.GetEntityView(
                    ShopsContainer,
                    string.Empty,
                    "Indexes",
                    string.Empty,
                    string.Empty);
                view.Should().NotBeNull();
                view.ChildViews.Should().NotBeNull();
                view.ChildViews.Should().NotBeEmpty();
                foreach (var entityView in view.ChildViews.OfType<EntityView>())
                {
                    view = new EntityView
                    {
                        Name = "Index",
                        DisplayName = "Index",
                        EntityId = entityView.EntityId,
                        Action = "DeleteSearchIndex"
                    };
                    var result = Proxy.DoCommand(ShopsContainer.DoAction(view));
                    result.Messages.Should().NotContainErrors();
                }

                Minions.RunFullIndexMinions(Constants.AWMinionsEnvironmentName);
                Minions.RunFullIndexMinions(Constants.HabitatMinionsEnvironmentName);
            }
        }
    }
}
