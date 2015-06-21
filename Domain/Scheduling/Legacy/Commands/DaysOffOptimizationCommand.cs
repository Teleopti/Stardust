using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class DaysOffOptimizationCommand
	{
		private readonly IDayOffOptimizationService _dayOffOptimizationService;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public DaysOffOptimizationCommand(IDayOffOptimizationService dayOffOptimizationService, IMatrixListFactory matrixListFactory, IScheduleDayEquator scheduleDayEquator)
		{
			_dayOffOptimizationService = dayOffOptimizationService;
			_matrixListFactory = matrixListFactory;
			_scheduleDayEquator = scheduleDayEquator;
		}

		public void Execute(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod)
		{
			IList<IScheduleMatrixPro> matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixList(scheduleDays, selectedPeriod);
			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization =
				matrixListForDayOffOptimization.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();

			//_dayOffOptimizationService.Execute();
		}
	}
}