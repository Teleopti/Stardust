using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IContractTimeShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IList<IScheduleMatrixPro> matrixList,
		                                                    IList<IShiftProjectionCache> shiftList,
		                                                    ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult);
	}
	
	public class ContractTimeShiftFilter : IContractTimeShiftFilter
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;

		public ContractTimeShiftFilter(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IList<IScheduleMatrixPro> matrixList,
		                                           IList<IShiftProjectionCache> shiftList,
		                                           ISchedulingOptions schedulingOptions, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (finderResult == null) return null;
			if (matrixList == null) return null;
			if (shiftList.Count == 0) return shiftList;
			
			IList<IShiftProjectionCache> workShifts = new List<IShiftProjectionCache>();
			MinMax<TimeSpan>? allowedMinMax = null;
			
		    foreach (var matrix in matrixList)
			{
				_workShiftMinMaxCalculator.ResetCache();
				var minMax = _workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, matrix, schedulingOptions);
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
			IList<IShiftProjectionCache> workShiftsWithinMinMax = new List<IShiftProjectionCache>();

			foreach (IShiftProjectionCache proj in shiftList)
			{
				if (allowedMinMax.Value.Contains(proj.WorkShiftProjectionContractTime))
					workShiftsWithinMinMax.Add(proj);
			}
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(
					string.Format(TeleoptiPrincipal.Current.Regional.Culture,
					              UserTexts.Resources.FilterOnContractTimeLimitationsWithParams, allowedMinMax.Value.Minimum,
					              allowedMinMax.Value.Maximum),
					cntBefore, workShiftsWithinMinMax.Count));

			return workShiftsWithinMinMax;
		}
	}
}
