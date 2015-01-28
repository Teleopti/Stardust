using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IShiftNudgeEarlier
	{
		bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class ShiftNudgeEarlier : IShiftNudgeEarlier
	{
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly ITeamBlockScheduler _teamBlockScheduler;

		public ShiftNudgeEarlier(ITeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator, ITeamBlockScheduler teamBlockScheduler)
		{
			_teamBlockClearer = teamBlockClearer;
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_teamBlockScheduler = teamBlockScheduler;
		}

		public bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var projectionPeriod = personAssignment.ProjectionService().CreateProjection().Period().Value;
			var shiftStartDate = projectionPeriod.StartDateTime.Date;
			var shiftEnd = projectionPeriod.EndDateTime;
			var adjustedEnd = shiftEnd.Add(TimeSpan.FromMinutes(-15)); //allways adjust 15 minutes regardless of interval length
			var dateOffset = (int)adjustedEnd.Date.Subtract(shiftStartDate).TotalDays;
			var shiftEndUserLocalDateTime = TimeZoneHelper.ConvertFromUtc(adjustedEnd, scheduleDay.TimeZone);
			var latestEndTime = shiftEndUserLocalDateTime.TimeOfDay.Add(TimeSpan.FromDays(dateOffset));

			rollbackService.ClearModificationCollection();
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

			var shiftDate = personAssignment.Date;
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(shiftDate, personAssignment.Person, teamBlockInfo,
				schedulingOptions);

			if (effectiveRestriction == null)
				return false;
			if (effectiveRestriction.EndTimeLimitation.StartTime.HasValue && effectiveRestriction.EndTimeLimitation.StartTime.Value > latestEndTime)
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

			bool result = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, shiftDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder,
				new ShiftNudgeDirective(adjustedEffectiveRestriction, ShiftNudgeDirective.NudgeDirection.Left));
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
