using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public class ShiftNudgeEarlier
	{
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public ShiftNudgeEarlier(TeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator, TeamBlockScheduler teamBlockScheduler, 
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IWorkShiftSelector workShiftSelector, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_teamBlockClearer = teamBlockClearer;
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_teamBlockScheduler = teamBlockScheduler;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}


		public bool Nudge(IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService,
			SchedulingOptions schedulingOptions, ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder,
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
				_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder));
			}


			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(schedulingResultStateHolder.Schedules, shiftDate, personAssignment.Person,
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

			//TODO: should pass in orginal assignments here to fix same issue as #45540 for shiftswithinday
			bool result = _teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, shiftDate, schedulingOptions,
				rollbackService, new DoNothingResourceCalculateDelayer(), schedulingResultStateHolder.AllSkillDays(), schedulingResultStateHolder.Schedules,
				new ShiftNudgeDirective(adjustedEffectiveRestriction, ShiftNudgeDirective.NudgeDirection.Left), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator);


			if (!result)
			{
				rollbackService.Rollback();
			}
			rollbackService.ClearModificationCollection();

			return result;
		}
	}
}
