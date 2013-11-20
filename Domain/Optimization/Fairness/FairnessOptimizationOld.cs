//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.Domain.Optimization.Fairness
//{
//    public interface IFairnessOptimizationOld
//    {
//        void Execute(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson,
//                                     IList<IScheduleDay> scheduleDays, IList<IShiftCategory> shiftCategories);

//        void ExecuteTwoConsectiveDays(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson,
//                                                      IList<IScheduleDay> scheduleDays);

//        void ExecuteBasedOnDayPriority(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson,
//                                                       IList<IScheduleDay> scheduleDays);
//    }

//    public class FairnessOptimizationOld : IFairnessOptimizationOld
//    {
//        private readonly IScheduleDayFinderWithLeastShiftCategory _dayFinderWithLeastShiftCategory;
//        private readonly IPrioritiseAgentForTeamBlock _prioritiseAgentByContract;
//        private readonly IPriortiseShiftCategoryForTeamBlock _priortiseShiftCategory;
//        private readonly IPriortiseWeekDay _priortizeWeekDay;
//        private readonly ISwapScheduleDays _swapScheduleDays;
//        private readonly IValidateScheduleDays _validateScheduleDays;

//        public FairnessOptimizationOld(IPriortiseWeekDay priortizeWeekDay, IPriortiseShiftCategoryForTeamBlock priortiseShiftCategory,
//                                    IPrioritiseAgentForTeamBlock prioritiseAgentByContract,
//                                    ISwapScheduleDays swapScheduleDays, IValidateScheduleDays validateScheduleDays,
//                                    IScheduleDayFinderWithLeastShiftCategory dayFinderWithLeastShiftCategory)
//        {
//            _priortizeWeekDay = priortizeWeekDay;
//            _priortiseShiftCategory = priortiseShiftCategory;
//            _prioritiseAgentByContract = prioritiseAgentByContract;
//            _swapScheduleDays = swapScheduleDays;
//            _validateScheduleDays = validateScheduleDays;
//            _dayFinderWithLeastShiftCategory = dayFinderWithLeastShiftCategory;
            
//        }

//        public void Execute(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson,
//                            IList<IScheduleDay> scheduleDays, IList<IShiftCategory> shiftCategories)
//        {
//            _priortiseShiftCategory.GetPriortiseShiftCategories(shiftCategories);
//            _prioritiseAgentByContract.GetPriortiseAgentByStartDate(selectedPerson);
//            IList<DateOnly> selectedCollection = selectedPeriod.DayCollection();
//            //Parallel.ForEach(selectedCollection, day => analyzeSampleDays(day, day, scheduleDays));
//            foreach (var day in selectedCollection)
//            {
//                analyzeSampleDays(day, day, scheduleDays);
//            }
//        }

//        public void ExecuteTwoConsectiveDays(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson,
//                                             IList<IScheduleDay> scheduleDays)
//        {
//            _prioritiseAgentByContract.GetPriortiseAgentByStartDate(selectedPerson);
//            IList<DateOnly> selectedCollection = selectedPeriod.DayCollection();
//            foreach (DateOnly day in selectedCollection)
//            {
//                DateOnly highPriorityDay = day;
//                DateOnly lowPriorityDay = day;
//                if (selectedCollection.Contains(day.AddDays(1)))
//                    lowPriorityDay = day.AddDays(1);
//                analyzeSampleDays(highPriorityDay, lowPriorityDay, scheduleDays);
//            }
//        }

//        public void ExecuteBasedOnDayPriority(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson,
//                                              IList<IScheduleDay> scheduleDays)
//        {
//            _prioritiseAgentByContract.GetPriortiseAgentByStartDate(selectedPerson);
//            _priortizeWeekDay.IdentifyPriority(scheduleDays);
//            for (int highPriority = _priortizeWeekDay.HigestPriority;
//                 highPriority >= _priortizeWeekDay.LowestPriority;
//                 highPriority --)
//            {
//                foreach (DateOnly highestPriorityDay in _priortizeWeekDay.GetDateListOnPriority(highPriority))
//                {
//                    for (int lowestPriority = _priortizeWeekDay.LowestPriority;
//                         lowestPriority <= highPriority;
//                         lowestPriority++)
//                    {
//                        //foreach (var lowestPriorityDay in _priortizeWeekDay.GetDateListOnPriority(lowestPriority))
//                        //{
//                        //    analyzeSampleDays(highestPriorityDay, lowestPriorityDay,scheduleDays );
//                        //}
//                        DateOnly day = highestPriorityDay;
//                        Parallel.ForEach(_priortizeWeekDay.GetDateListOnPriority(lowestPriority),
//                                         ch => analyzeSampleDays(day, ch, scheduleDays));
//                    }
//                }
//            }
//        }

//        private void analyzeSampleDays(DateOnly hightPriorityDate, DateOnly lowPriorityDate,
//                                       IList<IScheduleDay> scheduleDays)
//        {
//            for (int lowestPriorityOfPerson = _prioritiseAgentByContract.LowestPriority;
//                 lowestPriorityOfPerson <= _prioritiseAgentByContract.HigestPriority;
//                 lowestPriorityOfPerson++)
//            {
//                int personHighPriority = _prioritiseAgentByContract.HigestPriority;
//                while (lowestPriorityOfPerson <= personHighPriority)
//                {
//                    IScheduleDay highPriorityScheduleDay;
//                    IScheduleDay lowPriorirtyShceudleDay;
//                    if (getLowAndHighPriorityDaysOnDate(hightPriorityDate, lowPriorityDate, scheduleDays,
//                                                        personHighPriority, lowestPriorityOfPerson,
//                                                        out highPriorityScheduleDay, out lowPriorirtyShceudleDay))
//                    {
//                        personHighPriority--;
//                        continue;
//                    }
//                    if (_validateScheduleDays.Validate(highPriorityScheduleDay, lowPriorirtyShceudleDay))
//                    {
//                        _swapScheduleDays.Swap(highPriorityScheduleDay, lowPriorirtyShceudleDay);
//                        break;
//                    }
//                    personHighPriority--;
//                }
//            }
//        }

//        private bool getLowAndHighPriorityDaysOnDate(DateOnly hightPriorityDate, DateOnly lowPriorityDate,
//                                                     IList<IScheduleDay> scheduleDays, int personHighPriority,
//                                                     int lowestPriorityOfPerson,
//                                                     out IScheduleDay highPriorityScheduleDay,
//                                                     out IScheduleDay lowPriorirtyShceudleDay)
//        {
//            lowPriorirtyShceudleDay = null;
//            highPriorityScheduleDay =
//                _dayFinderWithLeastShiftCategory.GetScheduleDayWithTheLeastShiftCategory(hightPriorityDate, scheduleDays,
//                                                                                         personHighPriority,
//                                                                                         _priortiseShiftCategory
//                                                                                             .LowestPriority, _prioritiseAgentByContract.PersonOnPriority(personHighPriority),_priortiseShiftCategory);
//            if (highPriorityScheduleDay == null) return true;
//            int startIndex =
//                _priortiseShiftCategory.PriorityOfShiftCategory(
//                    highPriorityScheduleDay.GetEditorShift().ShiftCategory);
//            for (int i = lowestPriorityOfPerson; i <= personHighPriority; i++)
//            {
//                lowPriorirtyShceudleDay =
//                    _dayFinderWithLeastShiftCategory.GetScheduleDayWithTheLeastShiftCategory(lowPriorityDate,
//                                                                                             scheduleDays, i,
//                                                                                             startIndex + 1, _prioritiseAgentByContract.PersonOnPriority(i), _priortiseShiftCategory);
//                if (lowPriorirtyShceudleDay == null) continue;
//                break;
//            }
//            if (lowPriorirtyShceudleDay == null) return true;
//            return false;
//        }

//        ////schedule day finder based on low priority
//        //private IScheduleDay getScheduleDayWhileConsideringPriority(DateOnly priorityDate, IEnumerable<IScheduleDay> scheduleDays, int priorityOfPerson, int startIndex)
//        //{
//        //    var person = _prioritiseAgentByContract.PersonOnPriority(priorityOfPerson);
//        //    var scheduleListOnDate = scheduleDays.Where(s => s.DateOnlyAsPeriod.DateOnly == priorityDate).ToList();
//        //    IEnumerable<IScheduleDay> finalScheduleDays = new List<IScheduleDay>();
//        //    for (int priority = startIndex;
//        //         priority < _priortiseShiftCategory.HigestPriority;
//        //         priority++)
//        //    {
//        //        var shiftCategory = _priortiseShiftCategory.ShiftCategoryOnPriority(priority);
//        //        finalScheduleDays = scheduleListOnDate.Where(s => s.GetEditorShift().ShiftCategory == shiftCategory);
//        //        if (!finalScheduleDays.ToList().Any()) continue;
//        //        break;
//        //    }

//        //    return finalScheduleDays.FirstOrDefault(s => s.Person == person);
//        //}
//    }
//}