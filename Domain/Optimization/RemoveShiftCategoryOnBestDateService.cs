using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RemoveShiftCategoryOnBestDateService : IRemoveShiftCategoryOnBestDateService
    {
        private readonly IScheduleMatrixPro _scheduleMatrix;
        private readonly IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
        private readonly IScheduleDayService _scheduleService;

        private RemoveShiftCategoryOnBestDateService(){}

        public RemoveShiftCategoryOnBestDateService(IScheduleMatrixPro scheduleMatrix, 
                                                    IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, 
                                                    IScheduleDayService scheduleService) : this()
        {
            _scheduleMatrix = scheduleMatrix;
            _scheduleMatrixValueCalculator = scheduleMatrixValueCalculator;
            _scheduleService = scheduleService;
        }

        public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory)
        {
            IList<IScheduleDayPro> periodDays = new List<IScheduleDayPro>(_scheduleMatrix.EffectivePeriodDays);
            DateOnly start = periodDays[0].Day;
            DateOnly end = periodDays[periodDays.Count - 1].Day;
            DateOnlyPeriod period = new DateOnlyPeriod(start, end);

            return ExecuteOne(shiftCategory, period);
        }

        public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, DateOnlyPeriod period)
        {
            IList<IScheduleDayPro> daysToWorkWith = DaysToWorkWith(shiftCategory, period);
            IScheduleDayPro dayToRemove = FindDayToRemove(daysToWorkWith);
            if (dayToRemove == null)
                return null;

            _scheduleService.DeleteMainShift(new List<IScheduleDay> {dayToRemove.DaySchedulePart()});
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

            	double? current = _scheduleMatrixValueCalculator.DayValueForSkills(scheduleDayPro.Day, skillList);
                if(current.HasValue)
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