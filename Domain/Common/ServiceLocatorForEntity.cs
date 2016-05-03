using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static ICurrentPrincipalAuthorization _currentPrincipalAuthorization;

		public static ICurrentPrincipalAuthorization CurrentPrincipalAuthorization
		{
			get { return _currentPrincipalAuthorization ?? Security.Principal.CurrentPrincipalAuthorization.Make(); }
			set { _currentPrincipalAuthorization = value; }
		}

	}

	public static class ServiceLocatorForEntity
	{
		private static ICurrentBusinessUnit _currentBusinessUnit;
		private static IAppliedAlarm _appliedAlarm;
		private static IUpdatedBy _updatedBy;

		public static ICurrentBusinessUnit CurrentBusinessUnit
		{
			get { return _currentBusinessUnit ?? Common.CurrentBusinessUnit.Make(); }
			set { _currentBusinessUnit = value; }
		}

		public static IAppliedAlarm AppliedAlarm
		{
			get { return _appliedAlarm ?? new AllRulesIsAlarm(); }
			set { _appliedAlarm = value; }
		}

		public static IUpdatedBy UpdatedBy
		{
			get { return _updatedBy ?? Security.Principal.UpdatedBy.Make(); }
			set { _updatedBy = value; }
		}
	}
}
