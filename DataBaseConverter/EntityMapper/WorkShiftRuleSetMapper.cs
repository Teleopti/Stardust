using System;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Convert shiftclasses to workshift rules
    /// </summary>
    public class WorkShiftRuleSetMapper : Mapper<IWorkShiftRuleSet, global::Domain.ShiftClass>
    {
        

		#region Constructors (1) 

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="mappedObjectPair"></param>
        /// <param name="timeZone"></param>
        public WorkShiftRuleSetMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone) : base(mappedObjectPair, timeZone){}

		#endregion Constructors 

		#region Methods (5) 


		// Public Methods (1) 

        /// <summary>
        /// map method
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <returns></returns>
        public override IWorkShiftRuleSet Map(global::Domain.ShiftClass oldEntity)
        {
            if (oldEntity.Id < 0 || 
                oldEntity.StartAndLengthDefinition == null || 
                MappedObjectPair.ResolveActivity(oldEntity.BaseActivity)==null)
                return null;

            if (oldEntity.Unit.Deleted || !oldEntity.Category.InUse)
                return null;

            WorkShiftTemplateGenerator generator = createGenerator(oldEntity);

            WorkShiftRuleSet ruleSet = new WorkShiftRuleSet(generator); 
            ruleSet.Description = new Description(oldEntity.Name);
            
            if (oldEntity.ActivityCollection != null)
            {
                foreach (global::Domain.ShiftClassActivity shiftClassActivity in oldEntity.ActivityCollection)
                {
                    IWorkShiftExtender extenderToAdd;
                    extenderToAdd = createExtenderToAdd(shiftClassActivity);
                    ruleSet.AddExtender(extenderToAdd);
                }
            }
            if (oldEntity.ExcludedWeekDayCollection != null)
            {
                foreach (DayOfWeek dayOfWeek in oldEntity.ExcludedWeekDayCollection)
                {
                    ruleSet.AddAccessibilityDayOfWeek(dayOfWeek);
                }
            }
            if (oldEntity.ExcludedDateCollection != null)
            {
                foreach (DateTime excludedDate in oldEntity.ExcludedDateCollection)
                {
                    ruleSet.AddAccessibilityDate(new DateOnly(excludedDate));
                }
            }
            if (oldEntity.ActivityMinTimeCollection!=null)
            {
                foreach (var activityMinPeriod in oldEntity.ActivityMinTimeCollection)
                {
                    IWorkShiftLimiter workShiftLimiter =
                        new ActivityTimeLimiter(MappedObjectPair.ResolveActivity(activityMinPeriod.TheActivity),
                                                activityMinPeriod.MinTime, OperatorLimiter.GreaterThenEquals);
                    ruleSet.AddLimiter(workShiftLimiter);
                }
            }

            IWorkShiftLimiter workTimeLimiter =
                new ContractTimeLimiter(new TimePeriod(oldEntity.StartAndLengthDefinition.MinLength,
                                                       oldEntity.StartAndLengthDefinition.MaxLength),
                                                       oldEntity.StartAndLengthDefinition.EndTimeSegment);
                                                       
            ruleSet.AddLimiter(workTimeLimiter);
            return ruleSet;
        }



		// Private Methods (4) 


        private static TimeSpan checkEmptyTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan <=new TimeSpan(0))
                timeSpan = TimeSpan.FromTicks(1);

            return timeSpan;
        }

        private IWorkShiftExtender createExtenderToAdd(global::Domain.ShiftClassActivity shiftClassActivity)
        {
            IWorkShiftExtender extenderToAdd;
            IActivity activityActivity = MappedObjectPair.ResolveActivity(shiftClassActivity.Activity);
            TimePeriodWithSegment activityLength = createTimePeriodWithSegment(shiftClassActivity.MinLength,
                                                                               shiftClassActivity.MaxLength,
                                                                               shiftClassActivity.EndTimeSegment);
            if (!shiftClassActivity.AutoPosition)
            {
                TimePeriodWithSegment activityStartPeriod = createTimePeriodWithSegment(shiftClassActivity.EarliestStartTime,
                                                                                        shiftClassActivity.LatestStartTime,
                                                                                        shiftClassActivity.StartTimeSegment);
                if (shiftClassActivity.TimeRelativeStart)
                {
                    extenderToAdd = new ActivityRelativeStartExtender(activityActivity, activityLength, activityStartPeriod);
                }
                else
                {
                    extenderToAdd = new ActivityAbsoluteStartExtender(activityActivity, activityLength, activityStartPeriod);
                }
            }
            else
            {
                extenderToAdd = new AutoPositionedActivityExtender(activityActivity, activityLength, checkEmptyTimeSpan(shiftClassActivity.StartTimeSegment), (Byte)shiftClassActivity.NumPeriods);
            }
            return extenderToAdd;
        }

        private WorkShiftTemplateGenerator createGenerator(global::Domain.ShiftClass oldEntity)
        {
            IActivity activity = MappedObjectPair.ResolveActivity(oldEntity.BaseActivity);

            TimeSpan earliestStartTime = oldEntity.StartAndLengthDefinition.EarliestStartTime;
            TimeSpan latestStartTime = oldEntity.StartAndLengthDefinition.LatestStartTime;
           
            TimePeriodWithSegment startPeriod = createTimePeriodWithSegment(earliestStartTime,
                                                                            latestStartTime,
                                                                            oldEntity.StartAndLengthDefinition.StartTimeSegment);
            TimePeriodWithSegment endPeriod = createTimePeriodWithSegment(oldEntity.EarliestEnd, oldEntity.LatestEnd,
                                                                          oldEntity.EndSegment);

            return new WorkShiftTemplateGenerator(activity, startPeriod, endPeriod, MappedObjectPair.ShiftCategory.GetPaired(oldEntity.Category));
        }

        private static TimePeriodWithSegment createTimePeriodWithSegment(TimeSpan min, TimeSpan max, TimeSpan segment)
        {
            TimePeriod period = new TimePeriod(checkEmptyTimeSpan(min), checkEmptyTimeSpan(max));
            return new TimePeriodWithSegment(period, checkEmptyTimeSpan(segment));
        }


		#endregion Methods 

    }
}
