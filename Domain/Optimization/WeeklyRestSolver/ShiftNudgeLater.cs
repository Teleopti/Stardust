

using System;
using System.Collections.Generic;
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
			IList<IPerson> selectedPersons);
	}

	public class ShiftNudgeLater : IShiftNudgeLater
	{
		 private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly ITeamBlockScheduler _teamBlockScheduler;

		public ShiftNudgeLater(ITeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator, ITeamBlockScheduler teamBlockScheduler)
		{
			_teamBlockClearer = teamBlockClearer;
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_teamBlockScheduler = teamBlockScheduler;
		}

		public bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder, 
			IList<IPerson> selectedPersons)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var projectionPeriod = personAssignment.ProjectionService().CreateProjection().Period().Value;
			var shiftStartDate = projectionPeriod.StartDateTime.Date;
			var shiftStart = projectionPeriod.StartDateTime;
			var adjustedStart = shiftStart.Add(TimeSpan.FromMinutes(15)); //allways adjust 15 minutes regardless of interval length
			var dateOffset = (int)adjustedStart.Date.Subtract(shiftStartDate).TotalDays;
			var shiftStartUserLocalDateTime = TimeZoneHelper.ConvertFromUtc(adjustedStart, scheduleDay.TimeZone);
			var earliestStartTime = shiftStartUserLocalDateTime.TimeOfDay.Add(TimeSpan.FromDays(dateOffset));

			rollbackService.ClearModificationCollection();
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

			var shiftDate = personAssignment.Date;
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(shiftDate, personAssignment.Person, teamBlockInfo,
				schedulingOptions);

			if (effectiveRestriction.StartTimeLimitation.EndTime.HasValue && effectiveRestriction.StartTimeLimitation.EndTime.Value < earliestStartTime)
				return false;

			var adjustedStartTimeLimitation = new StartTimeLimitation(earliestStartTime, effectiveRestriction.StartTimeLimitation.EndTime);
			var adjustedEffectiveRestriction = new EffectiveRestriction(adjustedStartTimeLimitation,
				effectiveRestriction.EndTimeLimitation, effectiveRestriction.WorkTimeLimitation, effectiveRestriction.ShiftCategory,
				effectiveRestriction.DayOffTemplate, effectiveRestriction.Absence,
				new List<IActivityRestriction>(effectiveRestriction.ActivityRestrictionCollection));

			bool result = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, shiftDate, schedulingOptions,
				selectedPersons, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder,
				new ShiftNudgeDirective(adjustedEffectiveRestriction, ShiftNudgeDirective.NudgeDirection.Right));
			if (!result)
			{
				rollbackService.Rollback();
				var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
				foreach (var dateOnly in blockPeriod.DayCollection())
				{
					resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				}
				resourceCalculateDelayer.CalculateIfNeeded(blockPeriod.EndDate.AddDays(1), null);
			}
			rollbackService.ClearModificationCollection();
			
			return result;
		}
	}
}