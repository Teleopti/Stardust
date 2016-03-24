using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class ServiceLocatorForEntity
	{
		private static ICurrentBusinessUnit _currentBusinessUnit;
		private static IAppliedAlarm _appliedAlarm;

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
	}
}
