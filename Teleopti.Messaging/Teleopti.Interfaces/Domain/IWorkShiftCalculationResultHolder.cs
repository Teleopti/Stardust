
using Teleopti.Ccc.Secrets.WorkShiftCalculator;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
	public interface IWorkShiftCalculationResultHolder : IImprovableWorkShiftCalculation
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        double Value { get; set; }
        /// <summary>
        /// Gets or sets the shift projection.
        /// </summary>
        /// <value>The shift projection.</value>
        IShiftProjectionCache ShiftProjection { get; set; }
    }
}
