

using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IShiftNudgeLater
	{
		bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder,
			IOptimizationPreferences optimizationPreferences, bool firstNudge);
	}

	public class ShiftNudgeLater : IShiftNudgeLater
	{
		 private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

		public ShiftNudgeLater(ITeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator, ITeamBlockScheduler teamBlockScheduler, 
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter)
		{
			_teamBlockClearer = teamBlockClearer;
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_teamBlockScheduler = teamBlockScheduler;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
		}

		public bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder, IOptimizationPreferences optimizationPreferences, bool firstNudge)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var projectionPeriod = personAssignment.ProjectionService().CreateProjection().Period().Value;
			var shiftStartDate = projectionPeriod.StartDateTime.Date;
			var shiftStart = projectionPeriod.StartDateTime;
			var adjustedStart = shiftStart.Add(TimeSpan.FromMinutes(15)); //allways adjust 15 minutes regardless of interval length
			var dateOffset = (int)adjustedStart.Date.Subtract(shiftStartDate).TotalDays;
			var shiftStartUserLocalDateTime = TimeZoneHelper.ConvertFromUtc(adjustedStart, scheduleDay.TimeZone);
			var earliestStartTime = shiftStartUserLocalDateTime.TimeOfDay.Add(TimeSpan.FromDays(dateOffset));
			var shiftDate = personAssignment.Date;

			if (optimizationPreferences != null)
			{
				_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, optimizationPreferences, scheduleDay.GetEditorShift(), shiftDate);
			}

			rollbackService.ClearModificationCollection();
			if (firstNudge)
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
			else
			{
				_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo);
			}

			
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(shiftDate, personAssignment.Person, teamBlockInfo,
				schedulingOptions);

			if (effectiveRestriction.StartTimeLimitation.EndTime.HasValue && effectiveRestriction.StartTimeLimitation.EndTime.Value < earliestStartTime)
				return false;

			var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(shiftDate);

			foreach (var scheduleMatrixPro in matrixes)
			{
				var isLocked = scheduleMatrixPro.UnlockedDays.All(scheduleDayPro => !scheduleDayPro.Day.Equals(shiftDate));
				if (isLocked)
					return false;
			}

			var adjustedStartTimeLimitation = new StartTimeLimitation(earliestStartTime, effectiveRestriction.StartTimeLimitation.EndTime);
			var adjustedEffectiveRestriction = new EffectiveRestriction(adjustedStartTimeLimitation,
				effectiveRestriction.EndTimeLimitation, effectiveRestriction.WorkTimeLimitation, effectiveRestriction.ShiftCategory,
				effectiveRestriction.DayOffTemplate, effectiveRestriction.Absence,
				new List<IActivityRestriction>(effectiveRestriction.ActivityRestrictionCollection));

			bool result = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, shiftDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder,
				new ShiftNudgeDirective(adjustedEffectiveRestriction, ShiftNudgeDirective.NudgeDirection.Right));
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