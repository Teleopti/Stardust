using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Fairness
{
    public interface IScheduleDayFinderWithLeastShiftCategory
    {
        IScheduleDay GetScheduleDayWithTheLeastShiftCategory(DateOnly priorityDate,
                                                             IEnumerable<IScheduleDay> scheduleDays,
                                                             int priorityOfPerson, int lowestShiftCategoryPriority, IPerson person, IPriortiseShiftCategory priortiseShiftCategory);
    }

    public class ScheduleDayFinderWithLeastShiftCategory : IScheduleDayFinderWithLeastShiftCategory
    {
        //private readonly IPriortiseShiftCategory _priortiseShiftCategory;

        
        public IScheduleDay GetScheduleDayWithTheLeastShiftCategory(DateOnly priorityDate,
                                                                    IEnumerable<IScheduleDay> scheduleDays,
                                                                    int priorityOfPerson,
                                                                    int lowestShiftCategoryPriority, IPerson person, IPriortiseShiftCategory priortiseShiftCategory)
        {
            //IPerson person = _prioritiseAgentByContract.PersonOnPriority(priorityOfPerson);
            List<IScheduleDay> scheduleListOnDate =
                scheduleDays.Where(s => s.DateOnlyAsPeriod.DateOnly == priorityDate).ToList();
            IEnumerable<IScheduleDay> finalScheduleDays = new List<IScheduleDay>();
            for (int priority = lowestShiftCategoryPriority;
                 priority < priortiseShiftCategory.HigestPriority;
                 priority++)
            {
                IShiftCategory shiftCategory = priortiseShiftCategory.ShiftCategoryOnPriority(priority);
                finalScheduleDays = scheduleListOnDate.Where(s => s.GetEditorShift().ShiftCategory == shiftCategory).ToList();
                if (!finalScheduleDays.ToList().Any()) continue;
                break;
            }

            return finalScheduleDays.FirstOrDefault(s => s.Person == person);
        }
    }
}