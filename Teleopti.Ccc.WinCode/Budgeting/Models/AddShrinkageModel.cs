using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
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