using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting
{
    public interface IBudgetSettings : ISettingValue
    {
        ViewType SelectedView { get; set; }
    }

    [Serializable]
    public class BudgetSettings : SettingValue, IBudgetSettings
    {
        private ViewType _selectedView;

        public ViewType SelectedView
        {
            get { return _selectedView; }
            set { _selectedView = value; }
        }
    }
}
