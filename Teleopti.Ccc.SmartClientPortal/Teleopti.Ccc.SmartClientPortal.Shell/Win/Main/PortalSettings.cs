using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
    [Serializable]
    public class PortalSettings : SettingValue, IPortalSettings
    {
        private int _schedulerActionPaneHeight = 79;
        private int _peopleActionPaneHeight = 79;
        private int _intradayActionPaneHeight = 79;
        private int _forecasterActionPaneHeight = 79;
        private int _budgetingActionPaneHeight = 79;
        private int _payrollActionPaneHeight = 79;
        private int _moduleSelectorPanelHeight = 60;
        private string _lastModule;


        public int NumberOfVisibleGroupBars { get; set; }

        public int SchedulerActionPaneHeight
        {
            get { return _schedulerActionPaneHeight; }
            set { _schedulerActionPaneHeight = value; }
        }

        public string LastModule
        {
            get { return _lastModule; }
            set { _lastModule = value; }
        }

        public int ModuleSelectorPanelHeight
        {
            get { return _moduleSelectorPanelHeight; }
            set { _moduleSelectorPanelHeight = value; }
        }

        public int IntradayActionPaneHeight
        {
            get { return _intradayActionPaneHeight; }
            set { _intradayActionPaneHeight = value; }
        }

        public int ForecasterActionPaneHeight
        {
            get { return _forecasterActionPaneHeight; }
            set { _forecasterActionPaneHeight = value; }
        }

        public int BudgetingActionPaneHeight
        {
            get { return _budgetingActionPaneHeight; }
            set { _budgetingActionPaneHeight = value; }
        }

        public int PayrollActionPaneHeight
        {
            get { return _payrollActionPaneHeight; }
            set { _payrollActionPaneHeight = value; }
        }

        public int PeopleActionPaneHeight
        {
            get { return _peopleActionPaneHeight; }
            set { _peopleActionPaneHeight = value; }
        }
    }
}