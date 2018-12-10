using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionCreator : IEffectiveRestrictionCreator
	{
		private readonly IRestrictionExtractor _extractor; 

		public EffectiveRestrictionCreator(IRestrictionExtractor extractor)
		{
			_extractor = extractor;
		}

		public IEffectiveRestriction GetEffectiveRestriction(IScheduleDay part, SchedulingOptions options)
		{
			var result = _extractor.Extract(part);
			IEffectiveRestriction ret = result.CombinedRestriction(options);

			if (ret == null)
				return null;

			if (options.UseAvailability && ret.IsAvailabilityDay && ret.NotAvailable)
				ret.DayOffTemplate = options.DayOffTemplate;

			if (options.UseStudentAvailability && ret.NotAvailable)
				ret.DayOffTemplate = options.DayOffTemplate;

			return ret;
		}


		public IEffectiveRestriction GetEffectiveRestrictionForSinglePerson(
			IPerson person,
			DateOnly dateOnly,
			SchedulingOptions options,
			IScheduleDictionary scheduleDictionary)
		{
			InParameter.NotNull(nameof(scheduleDictionary), scheduleDictionary);
			IScheduleDay scheduleDay = scheduleDictionary[person].ScheduledDay(dateOnly);
			return GetEffectiveRestriction(scheduleDay, options);
		}

		public IEffectiveRestriction GetEffectiveRestriction(
            IEnumerable<IPerson> groupPersons, 
            DateOnly dateOnly, 
            SchedulingOptions options, 
            IScheduleDictionary scheduleDictionary)
		{
			InParameter.NotNull(nameof(scheduleDictionary), scheduleDictionary);

            IEffectiveRestriction ret = null;
			foreach (var person in groupPersons)
			{
				IScheduleDay scheduleDay = scheduleDictionary[person].ScheduledDay(dateOnly);
				var restriction = GetEffectiveRestriction(scheduleDay, options);
				if (restriction == null)
					return null;
			    if (ret != null)
			        ret = ret.Combine(restriction);
			    else
			        ret = restriction;
			    if (ret == null)
					return null;
			}

			return ret;
		}

		public static bool OptionsConflictWithRestrictions(SchedulingOptions options, IEffectiveRestriction effectiveRestriction)
		{
			if (options == null)
				return false;
			if (effectiveRestriction == null)
				return false;
			if (options.PreferencesDaysOnly && !effectiveRestriction.IsPreferenceDay)
				return true;
			if (options.RotationDaysOnly && !effectiveRestriction.IsRotationDay)
				return true;
			if (options.AvailabilityDaysOnly && !effectiveRestriction.IsAvailabilityDay)
				return true;

			return false;
		}
	}
}