using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortal.Common;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView
{
    public partial class PlanningTimeBankForm : BaseRibbonForm, IPlanningTimeBankView 
    {
        private readonly IPlanningTimeBankModel _model;
        private readonly PlanningTimeBankPresenter _presenter;

        public PlanningTimeBankForm(IPlanningTimeBankHelper planningTimeBankHelper, IPlanningTimeBankModel model)
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            _model = model;

            _presenter = new PlanningTimeBankPresenter(this, _model, planningTimeBankHelper, CultureInfo.CurrentCulture);
            _presenter.Initialize();

        }

        private void OfficeButtonSaveClick(object sender, EventArgs e)
        {
            buttonAdvSave.Focus();
            if (!_presenter.Save())
            {
                timeSpanTextBoxBalanceOut.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OfficeButtonCloseClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public void SetBalanceInText(string balanceInText)
        {
            autoLabelBalanceInInfo.Text = balanceInText;
        }

        public TimeSpan BalanceOut
        {
            get { return timeSpanTextBoxBalanceOut.Value; }
            set { timeSpanTextBoxBalanceOut.SetInitialResolution(value); }
        }

        public void SetMaxBalanceOut(TimeSpan maxBalanceOut)
        {
            timeSpanTextBoxBalanceOut.MaximumValue = maxBalanceOut;
        }

        public void SetInfoMinMaxBalance(string infoText)
        {
            autoLabelInfo.Text = infoText;
        }

        public void SetEnableStateOnSave(bool enabled)
        {
            buttonAdvSave.Enabled = enabled;
        }

        public void SetErrorText(string errorText)
        {
            autoLabelError.Text = errorText;
        }
    }
}
