using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizerHelperHelper
	{
		public void SetConsiderShortBreaks(IEnumerable<IPerson> persons, DateOnlyPeriod period, ISchedulingOptions options, IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak)
		{
			options.ConsiderShortBreaks = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(persons, period);
		}

		public IList<ISmartDayOffBackToLegalStateSolverContainer> CreateSmartDayOffSolverContainers(
			IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers = new List<ISmartDayOffBackToLegalStateSolverContainer>();
			foreach (var matrixOriginalStateContainer in matrixOriginalStateContainers)
			{
				var matrix = matrixOriginalStateContainer.ScheduleMatrix;
				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,matrix.EffectivePeriodDays.First().Day);

				ILockableBitArray bitArray = scheduleMatrixLockableBitArrayConverterEx.Convert(matrixOriginalStateContainer.ScheduleMatrix, dayOffOptimizationPreference.ConsiderWeekBefore, dayOffOptimizationPreference.ConsiderWeekAfter);
				IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(new OfficialWeekendDays(), new TrueFalseRandomizer());
				ISmartDayOffBackToLegalStateService solverService = new SmartDayOffBackToLegalStateService(cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);
				ISmartDayOffBackToLegalStateSolverContainer solverContainer = new SmartDayOffBackToLegalStateSolverContainer(matrixOriginalStateContainer, bitArray, solverService);
				solverContainers.Add(solverContainer);
			}

			return solverContainers;
		}

		public void SyncSmartDayOffContainerWithMatrix(
			ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer,
			IDayOffTemplate dayOffTemplate,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IScheduleTagSetter scheduleTagSetter,
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (backToLegalStateSolverContainer.Result)
			{
				IScheduleMatrixPro matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person, matrix.EffectivePeriodDays.First().Day);

				ILockableBitArray doArray = scheduleMatrixLockableBitArrayConverterEx.Convert(matrix, dayOffOptimizationPreference.ConsiderWeekBefore, dayOffOptimizationPreference.ConsiderWeekAfter);

				int bitArrayToMatrixOffset = 0;
				if (!dayOffOptimizationPreference.ConsiderWeekBefore)
					bitArrayToMatrixOffset = 7;

				for (int i = 0; i < doArray.Count; i++)
				{
					if (doArray[i] && !backToLegalStateSolverContainer.BitArray[i])
					{
						IScheduleDay part =
							matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
						part.DeleteDayOff();
						new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
					}
					else
					{
						if (!doArray[i] && backToLegalStateSolverContainer.BitArray[i])
						{
							IScheduleDay part =
								matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
							part.DeleteMainShift();
							part.CreateAndAddDayOff(dayOffTemplate);
							new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
						}
					}

				}
			}
		}
	}
}