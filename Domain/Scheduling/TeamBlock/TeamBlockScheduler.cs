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
								  DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService);

		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private bool _cancelMe;

		public TeamBlockScheduler(ITeamBlockSingleDayScheduler singleDayScheduler,
		                          ITeamBlockRoleModelSelector roleModelSelector,
									ITeamBlockClearer teamBlockClearer, ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_singleDayScheduler = singleDayScheduler;
			_roleModelSelector = roleModelSelector;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer,
										 ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod,
										 IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService)
		{
			_cancelMe = false;
			var selectedTeamMembers = teamBlockInfo.TeamInfo.GroupPerson.GroupMembers.Intersect(selectedPersons).ToList();
			if (selectedTeamMembers.IsEmpty())
				return true;
			var selectedBlockDays = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().Where(x => selectedPeriod.DayCollection().Contains(x)).ToList();
			DateOnly firstSelectedDayInBlock = selectedBlockDays.First();

			var roleModelShift = _roleModelSelector.Select(teamBlockInfo, firstSelectedDayInBlock, selectedTeamMembers.First(), schedulingOptions);
			if (roleModelShift == null)
			{
				OnDayScheduledFailed();
				return false;
			}

			
			bool success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedPeriod, selectedPersons, selectedBlockDays, roleModelShift);

			if (!success && _teamBlockSchedulingOptions.IsBlockWithSameShiftCategoryInvolved(schedulingOptions))
				{
					schedulingOptions.NotAllowedShiftCategories.Clear();
					while (roleModelShift != null && !success)
					{
						if(_cancelMe)
							break;

						_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo, selectedPersons);
						schedulingOptions.NotAllowedShiftCategories.Add(roleModelShift.TheMainShift.ShiftCategory);
						roleModelShift = _roleModelSelector.Select(teamBlockInfo, firstSelectedDayInBlock, selectedTeamMembers.First(), schedulingOptions);
						success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedPeriod, selectedPersons, selectedBlockDays, roleModelShift);
						
					}
					schedulingOptions.NotAllowedShiftCategories.Clear();
				}

			return success;

			
		}

		private bool tryScheduleBlock(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, List<DateOnly> selectedBlockDays, IShiftProjectionCache roleModelShift)
		{
			foreach (var day in selectedBlockDays)
			{
				if (_cancelMe)
				{
					return false;
				}

				_singleDayScheduler.DayScheduled += OnDayScheduled;
				bool successful = _singleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, selectedPersons, day,
					roleModelShift, selectedPeriod);
				_singleDayScheduler.DayScheduled -= OnDayScheduled;
				if (!successful)
				{
					OnDayScheduledFailed();
					return false;
				}
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

		public void OnDayScheduledFailed()
		{
			var args = new SchedulingServiceFailedEventArgs();
			var temp = DayScheduled;
			if (temp != null)
			{
				temp(this, args);
			}
			_cancelMe = args.Cancel;
		}
	}
}