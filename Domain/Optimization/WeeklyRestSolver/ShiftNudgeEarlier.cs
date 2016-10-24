using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IShiftNudgeEarlier
	{
		bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder,
			IOptimizationPreferences optimizationPreferences, bool firstNudge);
	}

	public class ShiftNudgeEarlier : IShiftNudgeEarlier
	{
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IWorkShiftSelector _workShiftSelector;

		public ShiftNudgeEarlier(ITeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator, ITeamBlockScheduler teamBlockScheduler, 
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IWorkShiftSelector workShiftSelector)
		{
			_teamBlockClearer = teamBlockClearer;
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_teamBlockScheduler = teamBlockScheduler;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_workShiftSelector = workShiftSelector;
		}


		public bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder,
			IOptimizationPreferences optimizationPreferences, bool firstNudge)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var projectionPeriodUtc = personAssignment.ProjectionService().CreateProjection().Period().Value;
			var timeZone = scheduleDay.TimeZone;
			var shiftEndUserLocalDateTime = projectionPeriodUtc.EndDateTimeLocal(timeZone);
			var shiftStartUserLocalDateTime = projectionPeriodUtc.StartDateTimeLocal(timeZone);
			var latestStartDateTime = shiftEndUserLocalDateTime.AddMinutes(-15); //allways adjust 15 minutes regardless of interval length
			var shiftDate = personAssignment.Date;
			
			var latestEndTime = latestStartDateTime.TimeOfDay;
				
			if (shiftEndUserLocalDateTime.AddTicks(-1).Date > shiftStartUserLocalDateTime.Date)
				latestEndTime = latestEndTime.Add(TimeSpan.FromDays(1));

			if (optimizationPreferences != null)
			{
				_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions,
					optimizationPreferences, scheduleDay.GetEditorShift(), shiftDate);
			}

			rollbackService.ClearModificationCollection();
			if (firstNudge)
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
			else
			{
				_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo);
			}


			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(shiftDate, personAssignment.Person,
				teamBlockInfo,
				schedulingOptions);

			if (effectiveRestriction.EndTimeLimitation.StartTime.HasValue &&
				effectiveRestriction.EndTimeLimitation.StartTime.Value > latestEndTime)
				return false;

			var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(shiftDate);

			foreach (var scheduleMatrixPro in matrixes)
			{
				var isLocked = scheduleMatrixPro.UnlockedDays.All(scheduleDayPro => !scheduleDayPro.Day.Equals(shiftDate));
				if (isLocked)
					return false;
			}

			var adjustedEndTimeLimitation = new EndTimeLimitation(effectiveRestriction.EndTimeLimitation.StartTime, latestEndTime);

			var adjustedEffectiveRestriction = new EffectiveRestriction(effectiveRestriction.StartTimeLimitation,
				adjustedEndTimeLimitation, effectiveRestriction.WorkTimeLimitation, effectiveRestriction.ShiftCategory,
				effectiveRestriction.DayOffTemplate, effectiveRestriction.Absence,
				new List<IActivityRestriction>(effectiveRestriction.ActivityRestrictionCollection));


			bool result = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, shiftDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(),
				new ShiftNudgeDirective(adjustedEffectiveRestriction, ShiftNudgeDirective.NudgeDirection.Left), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder));


			if (!result)
			{
				rollbackService.Rollback();
				var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
				foreach (var dateOnly in blockPeriod.DayCollection())
				{
					resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
				}
				resourceCalculateDelayer.CalculateIfNeeded(blockPeriod.EndDate.AddDays(1), null, false);
			}
			rollbackService.ClearModificationCollection();

			return result;
		}
	}
}
