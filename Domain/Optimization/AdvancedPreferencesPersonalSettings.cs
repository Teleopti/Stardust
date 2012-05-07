using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AdvancedPreferencesPersonalSettings : SettingValue, IPersonalSettings<IAdvancedPreferences>
	{

		private TargetValueOptions TargetValueCalculation { get; set; }
		private bool UseIntraIntervalDeviation { get; set; }
		private bool UseTweakedValues { get; set; }

		private bool UseMinimumStaffing { get; set; }
		private bool UseMaximumStaffing { get; set; }
		private bool UseMaximumSeats { get; set; }
		private bool DoNotBreakMaximumSeats { get; set; }

		private int RefreshScreenInterval { get; set; }


		public void MapTo(IAdvancedPreferences target)
		{
			target.TargetValueCalculation = TargetValueCalculation;
			target.UseIntraIntervalDeviation = UseIntraIntervalDeviation;
			target.UseTweakedValues = UseTweakedValues;

			target.UseMinimumStaffing = UseMinimumStaffing;
			target.UseMaximumStaffing = UseMaximumStaffing;
			target.UseMaximumSeats = UseMaximumSeats;
			target.DoNotBreakMaximumSeats = DoNotBreakMaximumSeats;

			target.RefreshScreenInterval = RefreshScreenInterval;
		}

		public void MapFrom(IAdvancedPreferences source)
		{
			TargetValueCalculation = source.TargetValueCalculation;
			UseIntraIntervalDeviation = source.UseIntraIntervalDeviation;
			UseTweakedValues = source.UseTweakedValues;

			UseMinimumStaffing = source.UseMinimumStaffing;
			UseMaximumStaffing = source.UseMaximumStaffing;
			UseMaximumSeats = source.UseMaximumSeats;
			DoNotBreakMaximumSeats = source.DoNotBreakMaximumSeats;

			RefreshScreenInterval = source.RefreshScreenInterval;
		}
	}
}
