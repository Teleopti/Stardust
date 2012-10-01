using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RemoveGroupShiftCategoryOnBestDateService : IRemoveShiftCategoryOnBestDateService
    {
        private readonly IScheduleMatrixPro _scheduleMatrix;
        private readonly IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculatorPro;
        private readonly IGroupSchedulingService _scheduleService;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        public RemoveGroupShiftCategoryOnBestDateService(IScheduleMatrixPro scheduleMatrix,
                                                         IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculatorPro,
                                                         IGroupSchedulingService scheduleService, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _scheduleMatrix = scheduleMatrix;
            _scheduleMatrixValueCalculatorPro = scheduleMatrixValueCalculatorPro;
            _scheduleService = scheduleService;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
        }

        public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, ISchedulingOptions schedulingOptions)
        {
            IList<IScheduleDayPro> periodDays = new List<IScheduleDayPro>(_scheduleMatrix.EffectivePeriodDays);
            DateOnly start = periodDays[0].Day;
            DateOnly end = periodDays[periodDays.Count - 1].Day;
            DateOnlyPeriod period = new DateOnlyPeriod(start, end);

            return ExecuteOne(shiftCategory, period, schedulingOptions);
        }

        public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, DateOnlyPeriod period, ISchedulingOptions schedulingOptions)
        {
            IList<IScheduleDayPro> daysToWorkWith = DaysToWorkWith(shiftCategory, period);
            IScheduleDayPro dayToRemove = FindDayToRemove(daysToWorkWith);
            if (dayToRemove == null)
                return null;
            var groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(dayToRemove.DaySchedulePart().Person,
                                                                                 dayToRemove.DaySchedulePart().
                                                                                     DateOnlyAsPeriod.DateOnly);

            _scheduleService.DeleteMainShift(new List<IScheduleDay> { dayToRemove.DaySchedulePart() }, schedulingOptions,groupPerson );
            return dayToRemove;
        }

        public bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory)
        {
            IScheduleDay part = scheduleDayPro.DaySchedulePart();
            if (part.SignificantPart() == SchedulePartView.MainShift)
            {
                if (part.AssignmentHighZOrder().MainShift.ShiftCategory.Equals(shiftCategory))
                    return true;
            }

            return false;
        }

        public IList<IScheduleDayPro> DaysToWorkWith(IShiftCategory shiftCategory, DateOnlyPeriod period)
        {
            IList<IScheduleDayPro> daysToWorkWith = new List<IScheduleDayPro>();
            foreach (var scheduleDayPro in _scheduleMatrix.UnlockedDays)
            {
                if (period.Contains(scheduleDayPro.Day) && IsThisDayCorrectShiftCategory(scheduleDayPro, shiftCategory))
                    daysToWorkWith.Add(scheduleDayPro);
            }

            return daysToWorkWith;
        }

        public IScheduleDayPro FindDayToRemove(IList<IScheduleDayPro> daysToWorkWith)
        {
            double min = double.MaxValue;
            IScheduleDayPro currentDay = null;

            foreach (var scheduleDayPro in daysToWorkWith)
            {
                IList<ISkill> skillList = new List<ISkill>();
                foreach (var personSkill in _scheduleMatrix.Person.Period(scheduleDayPro.Day).PersonSkillCollection)
                {
                    skillList.Add(personSkill.Skill);
                }

                double? current = _scheduleMatrixValueCalculatorPro.DayValueForSkills(scheduleDayPro.Day, skillList);
                if (current.HasValue)
                {
                    if (current.Value < min)
                    {
                        min = current.Value;
                        currentDay = scheduleDayPro;
                    }
                }
            }

            return currentDay;
        }
    }
}