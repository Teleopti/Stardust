using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public static class Pages
	{
		private static readonly ScenarioContextLazy<WeekSchedulePage> _weekSchedulePage =
			new ScenarioContextLazy<WeekSchedulePage>(() => Browser.Current.Page<WeekSchedulePage>());
		public static WeekSchedulePage WeekSchedulePage { get { return _weekSchedulePage.Value; } }

		private static readonly ScenarioContextLazy<MobileGlobalMenuPage> _mobileGlobalMenuPage =
		new ScenarioContextLazy<MobileGlobalMenuPage>(() => Browser.Current.Page<MobileGlobalMenuPage>());
		public static MobileGlobalMenuPage MobileGlobalMenuPage { get { return _mobileGlobalMenuPage.Value; } }

		private static readonly ScenarioContextLazy<GlobalMenuPage> _globalMenuPage =
		new ScenarioContextLazy<GlobalMenuPage>(() => Browser.Current.Page<GlobalMenuPage>());
		public static GlobalMenuPage GlobalMenuPage { get { return _globalMenuPage.Value; } }
		
		private static readonly ScenarioContextLazy<TeamSchedulePage> _teamSchedulePage =
			new ScenarioContextLazy<TeamSchedulePage>(() => Browser.Current.Page<TeamSchedulePage>());
		public static TeamSchedulePage TeamSchedulePage { get { return _teamSchedulePage.Value; } }

		public static WatiN.Core.Page Current { get { return ScenarioContext.Current.Value<WatiN.Core.Page>(); } }

		public static void NavigatingTo(WatiN.Core.Page page)
		{
			ScenarioContext.Current.Value(page);
		}

		public static PortalPage CurrentPortalPage { get { return Current as PortalPage; } }
		public static IOkButton CurrentOkButton { get { return Current as IOkButton; } }
		public static ICancelButton CurrentCancelButton { get { return Current as ICancelButton; } }
		public static IDateRangeSelector CurrentDateRangeSelector { get { return Current as IDateRangeSelector; } }
		public static IEditRequestPage CurrentEditRequestPage { get { return Current as IEditRequestPage; } }

	    public static IMessageReplyPage CurrentMessageReplyPage {get { return Current as IMessageReplyPage; }}
	}
}
