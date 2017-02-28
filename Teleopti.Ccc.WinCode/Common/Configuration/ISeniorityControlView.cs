using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public interface ISeniorityControlView
	{
		void RefreshListBoxWorkingDays(int newIndex);
		void RefreshListBoxShiftCategoryRank(int newIndex);
		void SetChangedInfo(ISeniorityWorkDayRanks workDayRanks);
	}
}
