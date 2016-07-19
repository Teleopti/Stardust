using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static ICurrentAuthorization _currentAuthorization;

		public static ICurrentAuthorization CurrentAuthorization
		{
			get { return _currentAuthorization ?? Security.Principal.CurrentAuthorization.Make(); }
			set { _currentAuthorization = value; }
		}

	}

	public static class ServiceLocatorForEntity
	{
		private static ICurrentBusinessUnit _currentBusinessUnit;
		private static ProperAlarm _appliedAlarm;
		private static IUpdatedBy _updatedBy;
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

		public static INow Now
		{
			get { return _now ?? new Now(); }
			set { _now = value; }
		}
	}
}
