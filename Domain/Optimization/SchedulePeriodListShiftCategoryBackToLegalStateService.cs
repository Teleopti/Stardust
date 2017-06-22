using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class SchedulePeriodListShiftCategoryBackToLegalStateService : ISchedulePeriodListShiftCategoryBackToLegalStateService
	{
		private readonly Func<ISchedulingResultStateHolder> _stateHolder;
		private readonly IScheduleMatrixValueCalculatorProFactory _scheduleMatrixValueCalculatorProFactory;
		private readonly IScheduleDayService _scheduleDayService;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;

		public SchedulePeriodListShiftCategoryBackToLegalStateService(
			Func<ISchedulingResultStateHolder> stateHolder,
			IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory,
			IScheduleDayService scheduleDayService,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback)
		{
			_stateHolder = stateHolder;
			_scheduleMatrixValueCalculatorProFactory = scheduleMatrixValueCalculatorProFactory;
			_scheduleDayService = scheduleDayService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute(IEnumerable<IScheduleMatrixPro> scheduleMatrixList, SchedulingOptions schedulingOptions)
		{
			IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator =
				BuildScheduleMatrixValueCalculator(
					_scheduleMatrixValueCalculatorProFactory,
					scheduleMatrixList,
					schedulingOptions,
					_stateHolder());

			ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder schedulePeriodBackToLegalStateServiceBuilder =
				CreateSchedulePeriodBackToLegalStateServiceBuilder(
					scheduleMatrixValueCalculator);

			foreach (IScheduleMatrixPro matrix in scheduleMatrixList)
			{
				ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
					new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
				ISchedulePeriodShiftCategoryBackToLegalStateService schedulePeriodBackToLegalStateService =
					schedulePeriodBackToLegalStateServiceBuilder.Build(matrix, schedulePartModifyAndRollbackService);
				schedulePeriodBackToLegalStateService.Execute(matrix.SchedulePeriod, schedulingOptions);
			}
		}

		public virtual ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder CreateSchedulePeriodBackToLegalStateServiceBuilder(
			IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator)
		{
			return new SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder(
									scheduleMatrixValueCalculator,
									() => _scheduleDayService);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public virtual IScheduleMatrixValueCalculatorPro BuildScheduleMatrixValueCalculator
			(IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory,
			IEnumerable<IScheduleMatrixPro> scheduleMatrixList,
			IMinMaxStaffing minMaxStaffing, 
			ISchedulingResultStateHolder stateHolder)
		{
			IList<DateOnly> days = new List<DateOnly>();
			foreach (IScheduleMatrixPro matrix in scheduleMatrixList)
			{
				foreach (IScheduleDayPro day in matrix.EffectivePeriodDays)
				{
					days.Add(day.Day);
				}
			}
			return scheduleMatrixValueCalculatorProFactory.CreateScheduleMatrixValueCalculatorPro
				(days, minMaxStaffing, stateHolder);
		}
	}
}
