using System;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday
{
    [Serializable]
    public class IntradaySetting
    {
        private string _name;
        private ChartSettings _chartSetting;
        private string _dockingState;

        public IntradaySetting(string name) : this()
        {
            _name = name;
            _chartSetting = new ChartSettings();
        }

        protected IntradaySetting()
        {}

        public virtual ChartSettings ChartSetting
        {
            get { return _chartSetting; }
            set { _chartSetting = value; }
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual string DockingState
        {
            get {
                return _dockingState;
            }
            set {
                _dockingState = value;
            }
        }
    }
}