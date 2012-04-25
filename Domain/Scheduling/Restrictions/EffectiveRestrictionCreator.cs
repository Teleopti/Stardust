using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionCreator : IEffectiveRestrictionCreator
	{
		private readonly IRestrictionExtractor _extractor; 
		private readonly IKeepRestrictionCreator _keepRestrictionCreator;

		public EffectiveRestrictionCreator(IRestrictionExtractor extractor, IKeepRestrictionCreator keepRestrictionCreator)
		{
			_extractor = extractor;
			_keepRestrictionCreator = keepRestrictionCreator;

		}

		public IEffectiveRestriction GetEffectiveRestriction(IScheduleDay part, ISchedulingOptions options)
		{
		    
		    _extractor.Extract(part);
		    IEffectiveRestriction ret = _extractor.CombinedRestriction(options);

			if (ret == null)
				return null;

		    if (options.UseAvailability && ret.IsAvailabilityDay && ret.NotAvailable)
		        ret.DayOffTemplate = options.DayOffTemplate;

			//TODO 12582 Ola temporary fix for bug 
			if (part != null && part.SignificantPart() != SchedulePartView.MainShift)
				return ret;

			if (options.RescheduleOptions == OptimizationRestriction.KeepShiftCategory)
			{
				IEffectiveRestriction keepShiftCatRestriction = _keepRestrictionCreator.CreateKeepShiftCategoryRestriction(part);
				ret = ret.Combine(keepShiftCatRestriction);
			}

			if (options.RescheduleOptions == OptimizationRestriction.KeepStartAndEndTime)
			{
				IEffectiveRestriction keepStartAndEndTimeRestriction =
					_keepRestrictionCreator.CreateKeepStartAndEndTimeRestriction(part);
				ret = ret.Combine(keepStartAndEndTimeRestriction);
			}

		    return ret;
		}

		public IEffectiveRestriction GetEffectiveRestriction(
            IList<IPerson> groupPersons, 
            DateOnly dateOnly, 
            ISchedulingOptions options, 
            IScheduleDictionary scheduleDictionary)
		{
			InParameter.NotNull("scheduleDictionary", scheduleDictionary);
            //IEffectiveRestriction ret = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
            //                                                          new WorkTimeLimitation(), null, null,
            //                                                          new List<IActivityRestriction>());
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

		public static bool OptionsConflictWithRestrictions(ISchedulingOptions options, IEffectiveRestriction effectiveRestriction)
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