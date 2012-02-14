using System;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class DayOffOptimizerPreferencesDialog : BaseRibbonForm
    {
        private IDayOffPlannerRules _ruleSet;

        public DayOffOptimizerPreferencesDialog(IDayOffPlannerRules ruleSet) :this()
        {
            _ruleSet = ruleSet;
        }

        private DayOffOptimizerPreferencesDialog()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public IDayOffPlannerRules RuleSet
        {
            get { return _ruleSet; }
        }

        private void SetColor()
        {
            BackColor = ColorHelper.DialogBackColor();
        }

        #region Handle events

        private void Form_Load(object sender, EventArgs e)
        {
            dayOffOptimizerPreferencesPanel1.Initialize(RuleSet);
            SetColor();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (dayOffOptimizerPreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer))
            {
                dayOffOptimizerPreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

    }
}
