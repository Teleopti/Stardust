using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public class AddShrinkageModel
    {
        public AddShrinkageModel(IBudgetGroup budgetGroup)
        {
            BudgetGroup = budgetGroup;
        }
        public IBudgetGroup BudgetGroup { get; set; }
        public ICustomShrinkage CustomShrinkageAdded { get; set; }
    }
}