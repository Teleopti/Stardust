using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class EffectiveRestrictionShiftFilter
	{
		public bool Filter(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction)
		{
			if (schedulingOptions == null) return false;
		    if (effectiveRestriction == null)
			{
				return false;
			}

		    if (effectiveRestriction.ShiftCategory != null && schedulingOptions.ShiftCategory != null)
			{
				if (effectiveRestriction.ShiftCategory.Id != schedulingOptions.ShiftCategory.Id)
				{
					return false;
				}
			}
			bool haveRestrictions = true;

			if (schedulingOptions.RotationDaysOnly && !effectiveRestriction.IsRotationDay)
			{
				haveRestrictions = false;
			}
			if (schedulingOptions.AvailabilityDaysOnly && !effectiveRestriction.IsAvailabilityDay)
			{
				haveRestrictions = false;
			}
			if (schedulingOptions.PreferencesDaysOnly && !effectiveRestriction.IsPreferenceDay)
			{
				haveRestrictions = false;
			}

			if (schedulingOptions.UsePreferencesMustHaveOnly && !effectiveRestriction.IsPreferenceDay)
			{
				haveRestrictions = false;
			}

			if (schedulingOptions.UseStudentAvailability && !effectiveRestriction.IsStudentAvailabilityDay)
			{
				haveRestrictions = false;
			}

			if (!haveRestrictions)
			{
				return false;
			}

			return true;
		}
	}
}
