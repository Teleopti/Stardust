using Autofac;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
	public class SchedulerSortCommandMapper
	{
		private readonly SchedulingScreenState _schedulerStateHolder;
		private readonly SchedulerSortCommandSetting _defaultSortSetting;
		private readonly ILifetimeScope _container;

		public SchedulerSortCommandMapper(
			SchedulingScreenState schedulerStateHolder, 
			SchedulerSortCommandSetting defaultSortSetting,
			ILifetimeScope container)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_defaultSortSetting = defaultSortSetting;
			_container = container;
		}

		public SchedulerSortCommandSetting GetSettingFromCommand(IScheduleSortCommand sortCommand)
		{
			if (sortCommand is SortByContractTimeAscendingCommand) return SchedulerSortCommandSetting.SortByContractTimeAscending;
			if (sortCommand is SortByContractTimeDescendingCommand) return SchedulerSortCommandSetting.SortByContractTimeDescending;
			if (sortCommand is SortByEndAscendingCommand) return SchedulerSortCommandSetting.SortByEndAscending;
			if (sortCommand is SortByEndDescendingCommand) return SchedulerSortCommandSetting.SortByEndDescending;
			if (sortCommand is SortByStartAscendingCommand) return SchedulerSortCommandSetting.SortByStartAscending;
			if (sortCommand is SortByStartDescendingCommand) return SchedulerSortCommandSetting.SortByStartDescending;
			if (sortCommand is SortBySeniorityRankingAscendingCommand) return SchedulerSortCommandSetting.SortBySeniorityRankingAscending;
			if (sortCommand is SortBySeniorityRankingDescendingCommand) return SchedulerSortCommandSetting.SortBySeniorityRankingDescending;
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
			if (setting == SchedulerSortCommandSetting.SortBySeniorityRankingAscending) return new SortBySeniorityRankingAscendingCommand(_schedulerStateHolder, _container.Resolve<IRankedPersonBasedOnStartDate>());
			if (setting == SchedulerSortCommandSetting.SortBySeniorityRankingDescending) return new SortBySeniorityRankingDescendingCommand(_schedulerStateHolder, _container.Resolve<IRankedPersonBasedOnStartDate>());
			return new NoSortCommand();
		}
	}
}
