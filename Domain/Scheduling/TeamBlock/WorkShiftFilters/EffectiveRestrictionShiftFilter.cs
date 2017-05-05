using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IEffectiveRestrictionShiftFilter
	{
		bool Filter(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, WorkShiftFinderResult finderResult);
	}
	
	public class EffectiveRestrictionShiftFilter : IEffectiveRestrictionShiftFilter
	{
		public bool Filter(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, WorkShiftFinderResult finderResult)
		{
			if (schedulingOptions == null) return false;
		    if (effectiveRestriction == null)
			{
				finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingRestrictions, 0,
																		 0));
				return false;
			}
			if (finderResult == null) return false;

		    if (effectiveRestriction.ShiftCategory != null && schedulingOptions.ShiftCategory != null)
			{
				if (effectiveRestriction.ShiftCategory.Id != schedulingOptions.ShiftCategory.Id)
				{
					finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingShiftCategories, 0,
																		 0));
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
				finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.NoRestrictionDefined, 0, 0));
				return false;
			}

			return true;
		}
	}
}
