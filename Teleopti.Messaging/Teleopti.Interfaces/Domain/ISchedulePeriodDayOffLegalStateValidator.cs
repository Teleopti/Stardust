using System.Collections;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Validates whether all the day offs are is legal state in the BitArray represented period.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ISchedulePeriodDayOffLegalStateValidator : IBinaryValidator
    {
    }
}