using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views
{
    public interface IBudgetGroupWeekView
    {
        IList<BudgetGroupWeekDetailModel> DataSource { get; set; }
    	void AddShrinkageRow(ICustomShrinkage customShrinkage);
    	void AddEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage);
        void Initialize();
    }
}
