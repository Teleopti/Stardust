using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class ServiceLocatorForEntity
	{
		private static ICurrentBusinessUnit _currentBusinessUnit;
		private static IAppliedAlarm _appliedAlarm;



		public static ICurrentBusinessUnit CurrentBusinessUnit { get { return _currentBusinessUnit ?? Common.CurrentBusinessUnit.Make(); } }
		public static IAppliedAlarm AppliedAlarm { get { return _appliedAlarm ?? new AllRulesIsAlarm(); } }



		public static void SetInstanceFromContainer(ICurrentBusinessUnit instance)
		{
			_currentBusinessUnit = instance;
		}

		public static void SetInstanceFromContainer(IAppliedAlarm instance)
		{
			_appliedAlarm = instance;
		}

	}
}