using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleMatrixListCreator : IScheduleMatrixListCreator
    {
        private readonly ISchedulingResultStateHolder _stateHolder;

        public ScheduleMatrixListCreator(ISchedulingResultStateHolder stateHolder)
        {
            _stateHolder = stateHolder;
        }

        public IList<IScheduleMatrixPro> CreateMatrixListFromScheduleParts(IEnumerable<IScheduleDay> scheduleDays)
        {
            IEnumerable<ISchedulePartExtractor> extractors = new UniqueSchedulePartExtractor().ExtractUniqueScheduleParts(scheduleDays);
            IList<IScheduleMatrixPro> matrixes = new List<IScheduleMatrixPro>();
            foreach (var schedulePartExtractor in extractors)
            {
                IPerson currentPerson = schedulePartExtractor.Person;
                IVirtualSchedulePeriod schedulePeriod = schedulePartExtractor.SchedulePeriod;
                DateOnlyPeriod actualSchedulePeriod = schedulePartExtractor.ActualSchedulePeriod;

                IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
                    new FullWeekOuterWeekPeriodCreator(actualSchedulePeriod,
                                                       currentPerson);
                matrixes.Add(new ScheduleMatrixPro(_stateHolder,
                                                   fullWeekOuterWeekPeriodCreator,
                                                   schedulePeriod));
            }
            return matrixes;
        }
    }
}
