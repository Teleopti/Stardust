﻿

using System;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions);
		bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, bool skipOffset);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private readonly IRestrictionAggregator _restrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ITeamScheduling _teamScheduling;
		private readonly IWorkShiftSelector _workShiftSelector;

		public TeamBlockScheduler(ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
			IRestrictionAggregator restrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			ITeamScheduling teamScheduling,
			IWorkShiftSelector workShiftSelector)
		{
			_skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
			_restrictionAggregator = restrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_teamScheduling = teamScheduling;
			_workShiftSelector = workShiftSelector;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions)
		{
			return ScheduleTeamBlock(teamBlockInfo, datePointer, schedulingOptions, false);
		}

		public bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, bool skipOffset)
		{
			if (teamBlockInfo == null)
				return false;

			//if teamBlockInfo is fully scheduled, continue;
		    if (isTeamBlockScheduled(teamBlockInfo)) return false;

			var restriction = _restrictionAggregator.Aggregate(teamBlockInfo,schedulingOptions);

			// (should we cover for max seats here?) ????
			var shifts = _workShiftFilterService.Filter(datePointer, teamBlockInfo, restriction,
														schedulingOptions, new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupPerson, datePointer));
			if (shifts == null || shifts.Count <= 0)
				return false;

			var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(teamBlockInfo);

			var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																						 schedulingOptions
																							 .WorkShiftLengthHintOption,
																						 schedulingOptions
																							 .UseMinimumPersons,
																						 schedulingOptions
																							 .UseMaximumPersons);
			//implement
			_teamScheduling.DayScheduled += dayScheduled;
			_teamScheduling.Execute(teamBlockInfo, bestShiftProjectionCache, skipOffset);
			_teamScheduling.DayScheduled -= dayScheduled;

			return true;
		}

        private bool isTeamBlockScheduled(ITeamBlockInfo teamBlockInfo)
        {
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    if (!matrix.GetScheduleDayByKey(day).DaySchedulePart().IsScheduled())
                        return false;
            return true;
        }

        void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			OnDayScheduled(e);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
		}
	}
}