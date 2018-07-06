using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public  class WorkShiftCalculationResult : IWorkShiftCalculationResultHolder
    {
        public double Value { get; set; }
		public ShiftProjectionCache ShiftProjection { get; set; }
		public int LengthInMinutes { get; set; }
		public IWorkShiftCalculatableProjection WorkShiftCalculatableProjection => ShiftProjection;
    }

	public class WorkShiftCalculationLengthAndValueResultComparer : IComparer<IWorkShiftCalculationResultHolder>
	{
		public int Compare(IWorkShiftCalculationResultHolder x, IWorkShiftCalculationResultHolder y)
		{
			if (x.LengthInMinutes > y.LengthInMinutes)
				return 1;
			if (x.LengthInMinutes < y.LengthInMinutes)
				return -1;
			if (x.Value > y.Value)
				return 1;
			if (x.Value < y.Value)
				return -1;
			return 0;
		}
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
