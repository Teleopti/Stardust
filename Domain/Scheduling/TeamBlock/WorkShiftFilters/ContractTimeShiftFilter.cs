using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IContractTimeShiftFilter
	{
		IList<ShiftProjectionCache> Filter(DateOnly dateOnly, IList<IScheduleMatrixPro> matrixList,
		                                                    IList<ShiftProjectionCache> shiftList,
		                                                    SchedulingOptions schedulingOptions, WorkShiftFinderResult finderResult);
	}
	
	public class ContractTimeShiftFilter : IContractTimeShiftFilter
	{
		private readonly Func<IWorkShiftMinMaxCalculator> _workShiftMinMaxCalculator;
		private readonly IUserCulture _userCulture;

		public ContractTimeShiftFilter(Func<IWorkShiftMinMaxCalculator> workShiftMinMaxCalculator, IUserCulture userCulture)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_userCulture = userCulture;
		}
		
		public IList<ShiftProjectionCache> Filter(DateOnly dateOnly, IList<IScheduleMatrixPro> matrixList,
		                                           IList<ShiftProjectionCache> shiftList,
		                                           SchedulingOptions schedulingOptions, WorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (finderResult == null) return null;
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
				finderResult.AddFilterResults(
					new WorkShiftFilterResult(UserTexts.Resources.NoShiftsThatMatchesTheContractTimeCouldBeFound, workShifts.Count, 0));
				return workShifts;
			}

			int cntBefore = shiftList.Count;
			var workShiftsWithinMinMax = shiftList.Where(s => allowedMinMax.Value.Contains(s.WorkShiftProjectionContractTime)).ToList();
			
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(
					string.Format(_userCulture.GetCulture(),
					              UserTexts.Resources.FilterOnContractTimeLimitationsWithParams, allowedMinMax.Value.Minimum,
					              allowedMinMax.Value.Maximum),
					cntBefore, workShiftsWithinMinMax.Count));

			return workShiftsWithinMinMax;
		}
	}
}
