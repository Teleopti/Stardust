using System.Collections.Generic;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public  class WorkShiftCalculationResult : IWorkShiftCalculationResultHolder
    {
        public double Value { get; set; }
		public IShiftProjectionCache ShiftProjection { get; set; }
	    public IWorkShiftCalculatableProjection WorkShiftCalculatableProjection { get { return ShiftProjection; } }
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
