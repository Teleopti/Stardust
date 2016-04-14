using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class ServiceLocatorForEntity
	{
		private static ICurrentBusinessUnit _currentBusinessUnit;
		private static IAppliedAlarm _appliedAlarm;
		private static ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		// these properties are injected by reflection in ServiceLocatorModule

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

		public static ICurrentTeleoptiPrincipal CurrentTeleoptiPrincipal
		{
			get { return _currentTeleoptiPrincipal ?? Security.Principal.CurrentTeleoptiPrincipal.Make(); }
			set { _currentTeleoptiPrincipal = value; }
		}
	}
}
