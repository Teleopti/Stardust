using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public static class Pages
	{
		private static readonly ScenarioContextLazy<RequestsPage> _requestsPage =
			new ScenarioContextLazy<RequestsPage>(() => Browser.Current.Page<RequestsPage>());
		public static RequestsPage RequestsPage { get { return _requestsPage.Value; } }

		private static readonly ScenarioContextLazy<TeamSchedulePage> _teamSchedulePage =
			new ScenarioContextLazy<TeamSchedulePage>(() => Browser.Current.Page<TeamSchedulePage>());
		public static TeamSchedulePage TeamSchedulePage { get { return _teamSchedulePage.Value; } }

		public static WatiN.Core.Page Current { get { return ScenarioContext.Current.Value<WatiN.Core.Page>(); } }

		public static void NavigatingTo(WatiN.Core.Page page)
		{
			ScenarioContext.Current.Value(page);
		}
	}
}
