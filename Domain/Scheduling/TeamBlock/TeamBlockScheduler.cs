using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions,
								  DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons);

		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ISameShiftCategoryBlockScheduler _sameShiftCategoryBlockScheduler;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private bool _cancelMe;

		public TeamBlockScheduler(ISameShiftCategoryBlockScheduler sameShiftCategoryBlockScheduler,
		                          ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
		                          ITeamBlockSingleDayScheduler singleDayScheduler,
		                          ITeamBlockRoleModelSelector roleModelSelector)
		{
			_sameShiftCategoryBlockScheduler = sameShiftCategoryBlockScheduler;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_singleDayScheduler = singleDayScheduler;
			_roleModelSelector = roleModelSelector;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer,
										 ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod,
										 IList<IPerson> selectedPersons)
		{

			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(schedulingOptions) ||
				_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(schedulingOptions))
			{
				_sameShiftCategoryBlockScheduler.DayScheduled += OnDayScheduled;
				bool successful = _sameShiftCategoryBlockScheduler.Schedule(teamBlockInfo, datePointer, schedulingOptions, selectedPeriod, selectedPersons);
				_sameShiftCategoryBlockScheduler.DayScheduled -= OnDayScheduled;
				return successful;
			}

			return scheduleSelectedDays(teamBlockInfo, datePointer, schedulingOptions, selectedPeriod, selectedPersons);
		}

		private bool scheduleSelectedDays(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions,
								  DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
		{
			var selectedTeamMembers = teamBlockInfo.TeamInfo.GroupPerson.GroupMembers.Intersect(selectedPersons).ToList();
			if (selectedTeamMembers.IsEmpty()) return true;
			var roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(), schedulingOptions);
			if (roleModelShift == null)
				return false;

			var selectedBlockDays = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().Where(x => selectedPeriod.DayCollection().Contains(x)).ToList();
			foreach (var day in selectedBlockDays)
			{
				if (_cancelMe)
				{
					return false;
				}

				_singleDayScheduler.DayScheduled += OnDayScheduled;
				bool successful = _singleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, selectedPersons, day, roleModelShift, selectedPeriod);
				_singleDayScheduler.DayScheduled -= OnDayScheduled;
				if (!successful) return false;
			}
			return true;
		}

		public void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
			_cancelMe = e.Cancel;
		}
	}
}