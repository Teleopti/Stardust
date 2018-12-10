using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamBlockClearer
	{
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly AffectedDates _affectedDates;

		public TeamBlockClearer(Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
								IDeleteSchedulePartService deleteSchedulePartService,
								IResourceCalculation resourceCalculation,
								AffectedDates affectedDates)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceCalculation = resourceCalculation;
			_affectedDates = affectedDates;
		}

		public void ClearTeamBlock(SchedulingOptions schedulingOptions,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		    ITeamBlockInfo teamBlock)
		{
			var toRemove = new HashSet<IScheduleDay>();

			var unlockedDates = teamBlock.BlockInfo.UnLockedDates();
			
			foreach (var unlockedDate in unlockedDates)
			{
				addDaysToRemove(teamBlock, unlockedDate, toRemove);
			}

			foreach (var scheduleDay in toRemove)
			{
				_deleteSchedulePartService.Delete(new[] { scheduleDay }, schedulePartModifyAndRollbackService);

				var resCalcData = _schedulingResultStateHolder().ToResourceOptimizationData(schedulingOptions.ConsiderShortBreaks, false);
				foreach (var date in _affectedDates.For(scheduleDay))
				{
					_resourceCalculation.ResourceCalculate(date, resCalcData);
				}
			}
		}

		public void ClearTeamBlockWithNoResourceCalculation(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ITeamBlockInfo teamBlock, INewBusinessRuleCollection businessRuleCollection)
		{
			var toRemove = new HashSet<IScheduleDay>();
			var unlockedDates = teamBlock.BlockInfo.UnLockedDates();

			foreach (var unlockedDate in unlockedDates)
			{
				addDaysToRemove(teamBlock, unlockedDate, toRemove);
			}

			_deleteSchedulePartService.Delete(toRemove, schedulePartModifyAndRollbackService, businessRuleCollection);
		}

		private static void addDaysToRemove(ITeamBlockInfo teamBlock, DateOnly dateOnly, ISet<IScheduleDay> toRemove)
		{
			var unlockedPersons = teamBlock.TeamInfo.UnLockedMembers(dateOnly);

			foreach (var person in unlockedPersons)
			{
				IScheduleMatrixPro matrix = teamBlock.TeamInfo.MatrixForMemberAndDate(person, dateOnly);
				if (matrix == null)	
					continue;

				IScheduleDayPro scheduleDayPro = matrix.GetScheduleDayByKey(dateOnly);
				if (!matrix.UnlockedDays.Contains(scheduleDayPro))	
					continue;

				IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
				SchedulePartView significant = scheduleDay.SignificantPart();
				if (significant == SchedulePartView.MainShift)
					toRemove.Add(scheduleDay);
			}
		}
	}
}
