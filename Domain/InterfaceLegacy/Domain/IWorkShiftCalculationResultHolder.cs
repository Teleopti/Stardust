﻿
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
        ShiftProjectionCache ShiftProjection { get; set; }

		int LengthInMinutes { get; set; }
    }
}
