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

		private static readonly ScenarioContextLazy<SignInPage> _signInPage =
			new ScenarioContextLazy<SignInPage>(() => Browser.Current.Page<SignInPage>());
		public static SignInPage SignInPage { get { return _signInPage.Value; } }

		private static readonly ScenarioContextLazy<MobileSignInPage> _mobileSignInPage =
		new ScenarioContextLazy<MobileSignInPage>(() => Browser.Current.Page<MobileSignInPage>());
		public static MobileSignInPage MobileSignInPage { get { return _mobileSignInPage.Value; } }

		private static readonly ScenarioContextLazy<MobileReportsPage> _mobileReportsPage =
		new ScenarioContextLazy<MobileReportsPage>(() => Browser.Current.Page<MobileReportsPage>());
		public static MobileReportsPage MobileReportsPage { get { return _mobileReportsPage.Value; } }

		private static readonly ScenarioContextLazy<RequestsPage> _requestsPage =
			new ScenarioContextLazy<RequestsPage>(() => Browser.Current.Page<RequestsPage>());
		public static RequestsPage RequestsPage { get { return _requestsPage.Value; } }

		private static readonly ScenarioContextLazy<PreferencePage> _preferencePage =
			new ScenarioContextLazy<PreferencePage>(() => Browser.Current.Page<PreferencePage>());
		public static PreferencePage PreferencePage { get { return _preferencePage.Value; } }

		private static readonly ScenarioContextLazy<TeamSchedulePage> _teamSchedulePage =
			new ScenarioContextLazy<TeamSchedulePage>(() => Browser.Current.Page<TeamSchedulePage>());
		public static TeamSchedulePage TeamSchedulePage { get { return _teamSchedulePage.Value; } }

		public static WatiN.Core.Page Current { get { return ScenarioContext.Current.Value<WatiN.Core.Page>(); } set { ScenarioContext.Current.Value(value); } }

		public static IDeleteButton CurrentDeleteButton { get { return Current as IDeleteButton; } }
		public static IOkButton CurrentOkButton { get { return Current as IOkButton; } }
		public static ICancelButton CurrentCancelButton { get { return Current as ICancelButton; } }
		public static IDateRangeSelector CurrentDateRangeSelector { get { return Current as IDateRangeSelector; } }
		public static IEditTextRequestPage CurrentEditTextRequestPage { get { return Current as IEditTextRequestPage; } }

		
	}
}