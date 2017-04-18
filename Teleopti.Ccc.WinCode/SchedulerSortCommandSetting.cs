using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
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
		SortByStartDescending = 7,
		SortBySeniorityRankingAscending = 8,
		SortBySeniorityRankingDescending = 9
	}
}