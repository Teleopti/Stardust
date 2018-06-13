using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static ICurrentAuthorization _currentAuthorization;
		private static ITimeZoneGuard _timeZoneGuard;
		private static CreateMergedCollection _createMergedCollection;

		public static ICurrentAuthorization CurrentAuthorization
		{
			get { return _currentAuthorization ?? Security.Principal.CurrentAuthorization.Make(); }
			set { _currentAuthorization = value; }
		}

		public static ITimeZoneGuard TimeZoneGuard
		{
			get { return _timeZoneGuard ?? (_timeZoneGuard = new TimeZoneGuard()); }
			set { _timeZoneGuard = value; }
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		public static CreateMergedCollection CreateMergedCollection
		{
			get => _createMergedCollection ?? new CreateMergedCollection();
			set => _createMergedCollection = value;
		}
	}

	public static class ServiceLocatorForEntity
	{
		private static ICurrentBusinessUnit _currentBusinessUnit;
		private static ProperAlarm _appliedAlarm;
		private static IUpdatedBy _updatedBy;
		private static ILoggedOnUserIsPerson _loggedOnUserIsPerson;
		private static INow _now;

		public static ICurrentBusinessUnit CurrentBusinessUnit
		{
			get { return _currentBusinessUnit ?? Common.CurrentBusinessUnit.Make(); }
			set { _currentBusinessUnit = value; }
		}

		public static ProperAlarm AppliedAlarm
		{
			get { return _appliedAlarm ?? new ProperAlarm(); }
			set { _appliedAlarm = value; }
		}

		public static IUpdatedBy UpdatedBy
		{
			get { return _updatedBy ?? Security.Principal.UpdatedBy.Make(); }
			set { _updatedBy = value; }
		}

		public static ILoggedOnUserIsPerson LoggedOnUserIsPerson
		{
			get { return _loggedOnUserIsPerson ?? new LoggedOnUserIsPerson(CurrentTeleoptiPrincipal.Make()); }
			set { _loggedOnUserIsPerson = value; }
		}

		public static INow Now
		{
			get { return _now ?? new Now(); }
			set { _now = value; }
		}
	}
}
