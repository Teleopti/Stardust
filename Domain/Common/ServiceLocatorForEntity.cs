using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class ServiceLocatorForEntity
	{
		private static ICurrentBusinessUnit _currentBusinessUnit;

		public static ICurrentBusinessUnit CurrentBusinessUnit
		{
			get
			{
				if (_currentBusinessUnit != null)
					return _currentBusinessUnit;
				return _currentBusinessUnit = Common.CurrentBusinessUnit.Make();
			}
		}

		public static void SetInstanceFromContainer(ICurrentBusinessUnit instance)
		{
			_currentBusinessUnit = instance;
		}

		public static IAppliedAlarm AppliedAlarm { get; private set; }

		public static void SetInstanceFromContainer(IAppliedAlarm instance)
		{
			AppliedAlarm = instance;
		}

	}
}