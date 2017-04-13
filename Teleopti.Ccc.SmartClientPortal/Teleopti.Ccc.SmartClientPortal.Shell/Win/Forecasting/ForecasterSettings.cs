using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting
{
    [Serializable]
    public class ForecasterSettings : SettingValue
    {
        private int _numericCellVariableDecimals = 1;
        private WorkingInterval _workingIntervalSkill = WorkingInterval.Day;
        private WorkingInterval _workingInterval = WorkingInterval.Day;
        private TemplateTarget _templateTarget = TemplateTarget.Workload;
        private WorkingInterval _chartInterval = WorkingInterval.Intraday;
        private bool _showGraph;
        private bool _showSkillView;

        public int NumericCellVariableDecimals
        {
            get { return _numericCellVariableDecimals; }
            set { _numericCellVariableDecimals = value.LimitRange(0,9); }
        }

        public WorkingInterval WorkingInterval
        {
            get { return _workingInterval; }
            set { _workingInterval = value; }
        }

        public TemplateTarget TemplateTarget
        {
            get { return _templateTarget; }
            set { _templateTarget = value; }
        }

        public WorkingInterval ChartInterval
        {
            get { return _chartInterval; }
            set { _chartInterval = value; }
        }

        public WorkingInterval WorkingIntervalSkill
        {
            get { return _workingIntervalSkill; }
            set { _workingIntervalSkill = value; }
        }

        public bool ShowGraph
        {
            get { return _showGraph; }
            set { _showGraph = value; }
        }

        public bool ShowSkillView
        {
            get { return _showSkillView; }
            set { _showSkillView = value; }
        }
    }
}