using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Budgeting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public class BudgetGroupMainModel
    {
        private readonly IBudgetSettingsProvider _budgetSettingsProvider;

        public BudgetGroupMainModel(IBudgetSettingsProvider budgetSettingsProvider)
        {
            _budgetSettingsProvider = budgetSettingsProvider;
        }

        public IScenario Scenario { get; set; }
        public DateOnlyPeriod Period { get; set; }
        public IBudgetGroup BudgetGroup { get; set; }

        public IBudgetSettings BudgetSettings
        {
            get { return _budgetSettingsProvider.BudgetSettings; }
        }

        public void SaveSettings()
        {
            _budgetSettingsProvider.Save();
        }
    }
}
