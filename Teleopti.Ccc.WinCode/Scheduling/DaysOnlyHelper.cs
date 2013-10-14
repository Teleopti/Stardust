using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class DaysOnlyHelper
	{
		private readonly ISchedulingOptions _schedulingOptions;

		public DaysOnlyHelper(ISchedulingOptions schedulingOptions)
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

		public ISchedulingOptions PreferenceOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as ISchedulingOptions;
				clonedOptions.UseRotations = false;
				clonedOptions.RotationDaysOnly = false;
				clonedOptions.UseAvailability = false;
				clonedOptions.AvailabilityDaysOnly = false;

				return clonedOptions;
			}
		}

		public ISchedulingOptions RotationOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as ISchedulingOptions;
				clonedOptions.UsePreferences = false;
				clonedOptions.PreferencesDaysOnly = false;
				clonedOptions.UsePreferencesMustHaveOnly = false;
				clonedOptions.UseAvailability = false;
				clonedOptions.AvailabilityDaysOnly = false;

				return clonedOptions;
			}
		}

		public ISchedulingOptions AvailabilityOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as ISchedulingOptions;
				clonedOptions.UsePreferences = false;
				clonedOptions.PreferencesDaysOnly = false;
				clonedOptions.UsePreferencesMustHaveOnly = false;
				clonedOptions.UseRotations = false;
				clonedOptions.RotationDaysOnly = false;

				return clonedOptions;
			}
		}

		public ISchedulingOptions NoOnlyOptions
		{
			get
			{
				var clonedOptions = _schedulingOptions.Clone() as ISchedulingOptions;
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
