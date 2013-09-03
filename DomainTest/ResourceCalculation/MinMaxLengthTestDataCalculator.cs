using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    /// <summary>
    /// Test data generator for resource calculators.
    /// </summary>
    /// <remarks>
    /// Returns a min, max 9 hours for the week-ends, and a min = 7, max = 9 hours for workdays. 
    /// </remarks>
    public sealed class MinMaxLengthTestDataCalculator : IMinMaxContractTimeCalculator
    {

        #region IMinMaxContractTimeCalculator Members
		
        MinMax<TimeSpan>? IMinMaxContractTimeCalculator.GetMinMaxContractTime(
            DateOnly workShiftDate,
            ISchedulingResultStateHolder resultStateHolder,	
            ISchedulingOptions schedulingOptions)
        {
            switch (workShiftDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    return new MinMax<TimeSpan>(TimeSpan.FromHours(9), TimeSpan.FromHours(9));
                default:
                    return new MinMax<TimeSpan>(TimeSpan.FromHours(7), TimeSpan.FromHours(9));
            }
        }

        #endregion
    }
}
