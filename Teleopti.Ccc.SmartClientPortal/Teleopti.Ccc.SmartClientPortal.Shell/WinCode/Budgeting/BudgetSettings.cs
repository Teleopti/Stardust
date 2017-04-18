using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
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
