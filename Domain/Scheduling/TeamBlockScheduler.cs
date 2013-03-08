

using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions);
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
			if (teamBlockInfo == null)
				return false;

			//if teamBlockInfo is fully scheduled, continue;

			//change signature
			var restriction = _restrictionAggregator.Aggregate(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection(),
															   teamBlockInfo.TeamInfo.GroupPerson,
															   teamBlockInfo.TeamInfo.MatrixesForGroup().ToList(),
															   schedulingOptions);

			// (should we cover for max seats here?) ????
			//change signature
			var shifts = _workShiftFilterService.Filter(datePointer, teamBlockInfo.TeamInfo.GroupPerson,
														teamBlockInfo.TeamInfo.MatrixesForGroup().ToList(), restriction,
														schedulingOptions);
			if (shifts == null || shifts.Count <= 0)
				return false;

			//change signature
			var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(teamBlockInfo.TeamInfo.GroupPerson,
																					 teamBlockInfo.BlockInfo.BlockPeriod
																								  .DayCollection());

			var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																						 schedulingOptions
																							 .WorkShiftLengthHintOption,
																						 schedulingOptions
																							 .UseMinimumPersons,
																						 schedulingOptions
																							 .UseMaximumPersons);
			//implement
			_teamScheduling.DayScheduled += dayScheduled;
			_teamScheduling.Execute(teamBlockInfo, bestShiftProjectionCache);
			_teamScheduling.DayScheduled -= dayScheduled;

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