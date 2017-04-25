using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class DaysOnlyHelper
	{
		private readonly SchedulingOptions _schedulingOptions;

		public DaysOnlyHelper(SchedulingOptions schedulingOptions)
		{
			_schedulingOptions = schedulingOptions;
		}

		public bool DaysOnly
		{
			get
			{
				return _schedulingOptions.PreferencesDaysOnly || _schedulingOptions.UsePreferencesMustHaveOnly || _schedulingOptions.RotationDaysOnly || _schedulingOptions.AvailabilityDaysOnly;
			}
		}

		public SchedulingOptions PreferenceOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as SchedulingOptions;
				clonedOptions.UseRotations = false;
				clonedOptions.RotationDaysOnly = false;
				clonedOptions.UseAvailability = false;
				clonedOptions.AvailabilityDaysOnly = false;

				return clonedOptions;
			}
		}

		public SchedulingOptions RotationOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as SchedulingOptions;
				clonedOptions.UsePreferences = false;
				clonedOptions.PreferencesDaysOnly = false;
				clonedOptions.UsePreferencesMustHaveOnly = false;
				clonedOptions.UseAvailability = false;
				clonedOptions.AvailabilityDaysOnly = false;

				return clonedOptions;
			}
		}

		public SchedulingOptions AvailabilityOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as SchedulingOptions;
				clonedOptions.UsePreferences = false;
				clonedOptions.PreferencesDaysOnly = false;
				clonedOptions.UsePreferencesMustHaveOnly = false;
				clonedOptions.UseRotations = false;
				clonedOptions.RotationDaysOnly = false;

				return clonedOptions;
			}
		}

		public SchedulingOptions NoOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as SchedulingOptions;
				clonedOptions.PreferencesDaysOnly = false;
				clonedOptions.UsePreferencesMustHaveOnly = false;
				clonedOptions.RotationDaysOnly = false;
				clonedOptions.AvailabilityDaysOnly = false;

				return clonedOptions;
			}
		}

		public bool UsePreferencesWithNoDaysOnly
		{
			get { return _schedulingOptions.UsePreferences && !_schedulingOptions.PreferencesDaysOnly && !_schedulingOptions.UsePreferencesMustHaveOnly; }
		}

		public bool UseRotationsWithNoDaysOnly
		{
			get { return _schedulingOptions.UseRotations && !_schedulingOptions.RotationDaysOnly; }
		}

		public bool UseAvailabilityWithNoDaysOnly
		{
			get { return _schedulingOptions.UseAvailability && !_schedulingOptions.AvailabilityDaysOnly; }
		}
	}
}
