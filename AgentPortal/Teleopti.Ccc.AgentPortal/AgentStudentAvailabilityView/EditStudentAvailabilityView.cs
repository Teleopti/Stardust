using System;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView
{
    public partial class EditStudentAvailabilityView : BaseUserControl, IEditStudentAvailabilityView
    {
        private readonly EditStudentAvailabilityPresenter _presenter;

        public EditStudentAvailabilityView()
        {
            InitializeComponent();
            if (DesignMode) return;

            if (!StateHolderReader.IsInitialized) return;

            _presenter = new EditStudentAvailabilityPresenter(this,
                                                                      new EditStudentAvailabilityModel(
                                                                          PermissionService.Instance(),
                                                                          ApplicationFunctionHelper.Instance()));
            SetTexts();

            AttachViewEventsToPresenter();
        }

        public EditStudentAvailabilityPresenter Presenter { get { return _presenter; } }


        private void AttachViewEventsToPresenter()
        {
            outlookTimePickerStartTime1.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerEndTime1.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            checkBoxAdvNextDay1.CheckStateChanged += (s, e) => _presenter.NotifyViewValueChanged();

            outlookTimePickerStartTime2.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerEndTime2.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            checkBoxAdvNextDay2.CheckStateChanged += (s, e) => _presenter.NotifyViewValueChanged();

            buttonAdvSave.Click += (s, e) => _presenter.Save();

            outlookTimePickerStartTime2.Visible = false;
            outlookTimePickerEndTime2.Visible = false;
            checkBoxAdvNextDay2.Visible = false;
            labelStartTime2.Visible = false;
            labelEndTime2.Visible = false;
        }

        private void OutlookTimePickerEndTime1Click(object sender, EventArgs e)
        {
            if (outlookTimePickerEndTime1.ListBox.SelectedIndex.Equals(0))
                outlookTimePickerEndTime1.SetTimeValue(outlookTimePickerStartTime1.TimeValue());
        }

        private void OutlookTimePickerEndTime2Click(object sender, EventArgs e)
        {
            if (outlookTimePickerEndTime2.ListBox.SelectedIndex.Equals(0))
                outlookTimePickerEndTime2.SetTimeValue(outlookTimePickerStartTime2.TimeValue());
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTipValidation.SetToolTip(checkBoxAdvNextDay1, Resources.NextDay);
            toolTipValidation.SetToolTip(checkBoxAdvNextDay2, Resources.NextDay);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;
            _presenter.Initialize();
        }

        public void HideView()
        {
            Visible = false;
        }

        public void AllowInput(bool enable)
        {
            Enabled = enable;
        }

        public bool SaveButtonEnabled
        {
            set { buttonAdvSave.Enabled = value; }
            get { return buttonAdvSave.Enabled; } 
        }

        public TimeSpan? StartTimeLimitation
        {
            set { outlookTimePickerStartTime1.SetTimeValue(value); }
            get { return outlookTimePickerStartTime1.TimeValue(); }
        }

        public TimeSpan? EndTimeLimitation
        {
            set { outlookTimePickerEndTime1.SetTimeValue(value); }
            get { return outlookTimePickerEndTime1.TimeValue(); }
        }

        public bool EndTimeLimitationNextDay
        {
            set { checkBoxAdvNextDay1.Checked = value; }
            get { return checkBoxAdvNextDay1.Checked; }
        }

        public TimeSpan? SecondStartTimeLimitation
        {
            set { outlookTimePickerStartTime2.SetTimeValue(value); }
            get { return outlookTimePickerStartTime2.TimeValue(); }
        }

        public TimeSpan? SecondEndTimeLimitation
        {
            set { outlookTimePickerEndTime2.SetTimeValue(value); }
            get { return outlookTimePickerEndTime2.TimeValue(); }
        }

        public bool SecondEndTimeLimitationNextDay
        {
            set { checkBoxAdvNextDay2.Checked = value; }
            get { return checkBoxAdvNextDay2.Checked; }
        }

        public string StartTimeLimitationErrorMessage
        {
            set { SetLimitationErrorMessage(outlookTimePickerStartTime1, value); }
            get { return errorProvider1.GetError(outlookTimePickerStartTime1); }
        }

        public string EndTimeLimitationErrorMessage
        {
            set { SetLimitationErrorMessage(outlookTimePickerEndTime1, value); }
            get { return errorProvider1.GetError(outlookTimePickerEndTime1); }
        }

        public string SecondStartTimeLimitationErrorMessage
        {
            set { SetLimitationErrorMessage(outlookTimePickerStartTime2, value); }
            get { return errorProvider1.GetError(outlookTimePickerStartTime2); }
        }

        public string SecondEndTimeLimitationErrorMessage
        {
            set { SetLimitationErrorMessage(outlookTimePickerEndTime2, value); }
            get { return errorProvider1.GetError(outlookTimePickerEndTime2); }
        }

        private void SetLimitationErrorMessage(Control timeMin, string value)
        {
            errorProvider1.SetIconPadding(timeMin, 4);
            errorProvider1.SetError(timeMin, value);
        }
    }
}