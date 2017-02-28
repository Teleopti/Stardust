using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorProFactory : IScheduleMatrixValueCalculatorProFactory
    {
	    private readonly IUserTimeZone _userTimeZone;

	    public ScheduleMatrixValueCalculatorProFactory(IUserTimeZone userTimeZone)
	    {
		    _userTimeZone = userTimeZone;
	    }

        public IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(IEnumerable<DateOnly> scheduleDays, IOptimizationPreferences optimizerPreferences, ISchedulingResultStateHolder stateHolder)
        {
            return new ScheduleMatrixValueCalculatorPro
                (scheduleDays,
                 optimizerPreferences,
                 stateHolder,
								 _userTimeZone);
        }
    }
}
