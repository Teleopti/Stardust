using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleMatrixListCreator : IScheduleMatrixListCreator
    {
        private readonly Func<ISchedulingResultStateHolder> _stateHolder;
	    private readonly IUniqueSchedulePartExtractor _uniqueSchedulePartExtractor;

	    public ScheduleMatrixListCreator(Func<ISchedulingResultStateHolder> stateHolder, IUniqueSchedulePartExtractor uniqueSchedulePartExtractor)
        {
	        _stateHolder = stateHolder;
	        _uniqueSchedulePartExtractor = uniqueSchedulePartExtractor;
        }

	    public IList<IScheduleMatrixPro> CreateMatrixListFromScheduleParts(IEnumerable<IScheduleDay> scheduleDays)
        {
            IEnumerable<ISchedulePartExtractor> extractors = _uniqueSchedulePartExtractor.ExtractUniqueScheduleParts(scheduleDays);
            IList<IScheduleMatrixPro> matrixes = new List<IScheduleMatrixPro>();
            foreach (var schedulePartExtractor in extractors)
            {
                IPerson currentPerson = schedulePartExtractor.Person;
                IVirtualSchedulePeriod schedulePeriod = schedulePartExtractor.SchedulePeriod;
                DateOnlyPeriod actualSchedulePeriod = schedulePartExtractor.ActualSchedulePeriod;

                IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
                    new FullWeekOuterWeekPeriodCreator(actualSchedulePeriod,
                                                       currentPerson);
                matrixes.Add(new ScheduleMatrixPro(_stateHolder(),
                                                   fullWeekOuterWeekPeriodCreator,
                                                   schedulePeriod));
            }
            return matrixes;
        }
    }
}
