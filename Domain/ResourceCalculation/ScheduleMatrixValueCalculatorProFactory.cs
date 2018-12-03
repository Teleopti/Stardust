using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorProFactory
    {
	    private readonly IUserTimeZone _userTimeZone;

	    public ScheduleMatrixValueCalculatorProFactory(IUserTimeZone userTimeZone)
	    {
		    _userTimeZone = userTimeZone;
	    }

        public IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(IEnumerable<DateOnly> scheduleDays, SchedulingOptions schedulingOptions, ISchedulingResultStateHolder stateHolder)
        {
            return new ScheduleMatrixValueCalculatorPro
                (scheduleDays,
                 schedulingOptions,
                 stateHolder,
								 _userTimeZone);
        }
    }
}
