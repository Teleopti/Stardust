using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
	public interface IShiftCategoryFairnessContractToleranceChecker
	{
		bool IsOutsideTolerance(IEnumerable<IScheduleMatrixPro> matrixListForFairnessOptimization, IPerson person);
	}

	public class ShiftCategoryFairnessContractToleranceChecker : IShiftCategoryFairnessContractToleranceChecker
	{
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;

		public ShiftCategoryFairnessContractToleranceChecker(ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator)
		{
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool IsOutsideTolerance(IEnumerable<IScheduleMatrixPro> matrixListForFairnessOptimization, IPerson person)
		{
			foreach (var scheduleMatrixPro in matrixListForFairnessOptimization)
			{
				if (scheduleMatrixPro.Person == person)
				{
					var tolerance = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(scheduleMatrixPro);
					var contractTime = TimeSpan.Zero;
					foreach (var dayPro in scheduleMatrixPro.EffectivePeriodDays)
					{
						contractTime = contractTime.Add(dayPro.DaySchedulePart().ProjectionService().CreateProjection().ContractTime());
					}
					return !tolerance.ContainsPart(contractTime);
				}
			}

			return false;
		}
	}
}