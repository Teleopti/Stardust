using System;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands
{
	[Serializable]
	public enum SchedulerSortCommandSetting
	{
		None = 0,
		NoSortCommand = 1,
		SortByContractTimeAscending = 2,
		SortByContractTimeDescending = 3,
		SortByEndAscending = 4,
		SortByEndDescending = 5,
		SortByStartAscending = 6,
		SortByStartDescending = 7
	}

	public class SchedulerSortCommandMapper
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly SchedulerSortCommandSetting _defaultSortSetting;

		public SchedulerSortCommandMapper(ISchedulerStateHolder schedulerStateHolder, SchedulerSortCommandSetting defaultSortSetting)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_defaultSortSetting = defaultSortSetting;
		}

		public SchedulerSortCommandSetting GetSettingFromCommand(IScheduleSortCommand sortCommand)
		{
			if (sortCommand is SortByContractTimeAscendingCommand) return SchedulerSortCommandSetting.SortByContractTimeAscending;
			if (sortCommand is SortByContractTimeDescendingCommand) return SchedulerSortCommandSetting.SortByContractTimeDescending;
			if (sortCommand is SortByEndAscendingCommand) return SchedulerSortCommandSetting.SortByEndAscending;
			if (sortCommand is SortByEndDescendingCommand) return SchedulerSortCommandSetting.SortByEndDescending;
			if (sortCommand is SortByStartAscendingCommand) return SchedulerSortCommandSetting.SortByStartAscending;
			if (sortCommand is SortByStartDescendingCommand) return SchedulerSortCommandSetting.SortByStartDescending;

			return _defaultSortSetting;
		}

		public IScheduleSortCommand GetCommandFromSetting(SchedulerSortCommandSetting setting)
		{
			if (setting == SchedulerSortCommandSetting.SortByStartAscending) return new SortByStartAscendingCommand(_schedulerStateHolder);
			if (setting == SchedulerSortCommandSetting.SortByStartDescending) return new SortByStartDescendingCommand(_schedulerStateHolder);
			if (setting == SchedulerSortCommandSetting.SortByEndDescending) return new SortByEndDescendingCommand(_schedulerStateHolder);
			if (setting == SchedulerSortCommandSetting.SortByEndAscending) return new SortByEndAscendingCommand(_schedulerStateHolder);
			if (setting == SchedulerSortCommandSetting.SortByContractTimeAscending) return new SortByContractTimeAscendingCommand(_schedulerStateHolder);
			if (setting == SchedulerSortCommandSetting.SortByContractTimeDescending) return new SortByContractTimeDescendingCommand(_schedulerStateHolder);

			return new NoSortCommand(_schedulerStateHolder);
		}
	}
}
