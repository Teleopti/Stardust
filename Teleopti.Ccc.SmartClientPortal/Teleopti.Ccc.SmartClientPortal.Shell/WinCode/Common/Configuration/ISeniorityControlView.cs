using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public interface ISeniorityControlView
	{
		void RefreshListBoxWorkingDays(int newIndex);
		void RefreshListBoxShiftCategoryRank(int newIndex);
		void SetChangedInfo(ISeniorityWorkDayRanks workDayRanks);
	}
}
