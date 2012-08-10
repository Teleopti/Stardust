using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Intraday
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