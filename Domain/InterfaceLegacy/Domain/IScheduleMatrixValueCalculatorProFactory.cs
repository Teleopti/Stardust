using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Factory for <see cref="IScheduleMatrixValueCalculatorPro"/> class.
    /// </summary>
    public interface IScheduleMatrixValueCalculatorProFactory
    {
	    IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(IEnumerable<DateOnly> scheduleDays, IMinMaxStaffing minMaxStaffing, ISchedulingResultStateHolder stateHolder);
    }
}