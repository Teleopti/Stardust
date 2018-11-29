using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class ContractTimeShiftFilter
	{
		private readonly Func<IWorkShiftMinMaxCalculator> _workShiftMinMaxCalculator;

		public ContractTimeShiftFilter(Func<IWorkShiftMinMaxCalculator> workShiftMinMaxCalculator)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		}
		
		public IList<ShiftProjectionCache> Filter(DateOnly dateOnly, IList<IScheduleMatrixPro> matrixList,
		                                           IList<ShiftProjectionCache> shiftList, SchedulingOptions schedulingOptions)
		{
			if (shiftList == null) return null;
			if (matrixList == null) return null;
			if (shiftList.Count == 0) return shiftList;
			if (schedulingOptions.AllowBreakContractTime) return shiftList;
			
			IList<ShiftProjectionCache> workShifts = new List<ShiftProjectionCache>();
			MinMax<TimeSpan>? allowedMinMax = null;
			
		    foreach (var matrix in matrixList)
			{
				_workShiftMinMaxCalculator().ResetCache();
				var minMax = _workShiftMinMaxCalculator().MinMaxAllowedShiftContractTime(dateOnly, matrix, schedulingOptions);
				if (minMax.HasValue)
				{
					if (minMax.Value.Minimum == TimeSpan.Zero && minMax.Value.Maximum == TimeSpan.Zero)
						continue;
				}
				else
				{
					continue;
				}
				if (!allowedMinMax.HasValue)
					allowedMinMax = minMax;
				else
				{
					if (minMax.Value.Minimum < allowedMinMax.Value.Minimum)
						allowedMinMax = new MinMax<TimeSpan>(minMax.Value.Minimum, allowedMinMax.Value.Maximum);
					if (minMax.Value.Maximum < allowedMinMax.Value.Maximum && minMax.Value.Maximum > allowedMinMax.Value.Minimum)
						allowedMinMax = new MinMax<TimeSpan>(allowedMinMax.Value.Minimum, minMax.Value.Maximum);
				}
			}

			if (!allowedMinMax.HasValue)
			{
				return workShifts;
			}

			var workShiftsWithinMinMax = shiftList.Where(s => allowedMinMax.Value.Contains(s.WorkShiftProjectionContractTime())).ToList();

			return workShiftsWithinMinMax;
		}
	}
}
