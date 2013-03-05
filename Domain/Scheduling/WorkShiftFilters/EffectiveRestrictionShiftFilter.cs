﻿using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters
{
	public interface IEffectiveRestrictionShiftFilter
	{
		bool Filter(ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IWorkShiftFinderResult finderResult);
	}
	
	public class EffectiveRestrictionShiftFilter : IEffectiveRestrictionShiftFilter
	{
		public bool Filter(ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IWorkShiftFinderResult finderResult)
		{
			if (effectiveRestriction == null)
			{
				finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingRestrictions, 0,
																		 0));
				return false;
			}

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
