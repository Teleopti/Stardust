using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public  class WorkShiftCalculationResult : IWorkShiftCalculationResultHolder
    {
        public double Value { get; set; }
		public ShiftProjectionCache ShiftProjection { get; set; }
	    public IWorkShiftCalculatableProjection WorkShiftCalculatableProjection => ShiftProjection;
    }

	public class WorkShiftCalculationResultComparer : IComparer<IWorkShiftCalculationResultHolder>
	{
		public int Compare(IWorkShiftCalculationResultHolder x, IWorkShiftCalculationResultHolder y)
		{
			if (x.Value > y.Value)
				return 1;
			if (x.Value < y.Value)
				return -1;
			return 0;
		}
	}
}
