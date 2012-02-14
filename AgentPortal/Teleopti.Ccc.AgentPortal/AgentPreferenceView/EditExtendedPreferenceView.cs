using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Common.Controls;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView
{
    public partial class EditExtendedPreferenceView : BaseUserControl, IEditExtendedPreferenceView
    {
        private readonly EditExtendedPreferencePresenter _presenter;

        public EditExtendedPreferenceView()
        {
            InitializeComponent();
            if (DesignMode) return;

            if (!StateHolderReader.IsInitialized) return;

            _presenter = new EditExtendedPreferencePresenter(this,
                                                             new ExtendedPreferenceModel(
                                                                 StateHolder.Instance.StateReader.SessionScopeData,
                                                                 PermissionService.Instance(),
                                                                 ApplicationFunctionHelper.Instance()));
            SetTexts();

            AttachViewEventsToPresenter();
        }

        public EditExtendedPreferencePresenter Presenter { get { return _presenter; } }

        private void AttachViewEventsToPresenter()
        {
            comboBoxAdvDayOff.SelectedIndexChanged += (s, e) => _presenter.NotifyDayOffChanged(comboBoxAdvDayOff.SelectedItem as DayOff);
            comboBoxAdvShiftCategory.SelectedIndexChanged += (s, e) => _presenter.NotifyShiftCategoryChanged(comboBoxAdvShiftCategory.SelectedItem as ShiftCategory);
            comboBoxAdvAbsence.SelectedIndexChanged += (s, e) => _presenter.NotifyAbsenceChanged(comboBoxAdvAbsence.SelectedItem as Absence);

            outlookTimePickerStartTimeMin.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerStartTimeMax.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerEndTimeMin.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            checkBoxAdvEndTimeMin.CheckStateChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerEndTimeMax.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            checkBoxAdvEndTimeMin.CheckStateChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerWorkTimeMax.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerWorkTimeMin.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();

            comboBoxAdvActivity.SelectedIndexChanged +=
                (s, e) => _presenter.NotifyActivityChanged(comboBoxAdvActivity.SelectedItem as Activity);
            outlookTimePickerActivityStartTimeMin.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerActivityStartTimeMax.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerActivityEndTimeMin.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerActivityEndTimeMax.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerActivityTimeMax.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();
            outlookTimePickerActivityTimeMin.TextChanged += (s, e) => _presenter.NotifyViewValueChanged();

            buttonAdvSave.Click += (s, e) => _presenter.Save();
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTipValidation.SetToolTip(checkBoxAdvEndTimeMin, Resources.NextDay);
            toolTipValidation.SetToolTip(checkBoxAdvEndTimeMax, Resources.NextDay);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;
            _presenter.Initialize();
        }

        #region IEditExtendedPreferenceView

        public void PopulateShiftCategories(IEnumerable<ShiftCategory> shiftCategories)
        {
            comboBoxAdvShiftCategory.DisplayMember = "Name";
            comboBoxAdvShiftCategory.ValueMember = "Id";
            comboBoxAdvShiftCategory.DataSource = shiftCategories;
        }

        public void PopulateDaysOff(IEnumerable<DayOff> daysOff)
        {
            comboBoxAdvDayOff.DisplayMember = "Name";
            comboBoxAdvDayOff.ValueMember = "Id";
            comboBoxAdvDayOff.DataSource = daysOff;
        }

        public void PopulateAbsences(IEnumerable<Absence> absences)
        {
            comboBoxAdvAbsence.DisplayMember = "Name";
            comboBoxAdvAbsence.ValueMember = "Id";
            comboBoxAdvAbsence.DataSource = absences;
        }

        public void PopulateActivities(IEnumerable<Activity> activities)
        {
            comboBoxAdvActivity.DisplayMember = "Name";
            comboBoxAdvActivity.ValueMember = "Id";
            comboBoxAdvActivity.DataSource = activities;
        }

        public void HideView()
        {
            Visible = false;
        }

        public bool ActivityViewVisible
        {
            set
            {
                tableLayoutPanelExtendedPreferenceActivity.Visible = value;
                if (value)
                {

                    tableLayoutPanelControlLayout.RowStyles[1].Height = 50;
                    tableLayoutPanelControlLayout.RowStyles[1].SizeType = SizeType.Percent;
                }
                else
                {
                    tableLayoutPanelControlLayout.Controls.Remove(tableLayoutPanelExtendedPreferenceActivity);
                    tableLayoutPanelControlLayout.RowStyles[1].Height = 0;
                    tableLayoutPanelControlLayout.RowStyles[1].SizeType = SizeType.Absolute;
                }
            }
            get { return tableLayoutPanelExtendedPreferenceActivity.Visible; }
        }

        public bool DayOffEnabled { set { comboBoxAdvDayOff.Enabled = value; } get { return comboBoxAdvDayOff.Enabled; } }

        public bool ShiftCategoryEnabled { set { comboBoxAdvShiftCategory.Enabled = value; } get { return comboBoxAdvShiftCategory.Enabled; } }

        public bool AbsenceEnabled { set { comboBoxAdvAbsence.Enabled = value; } get { return comboBoxAdvAbsence.Enabled; } }

        public bool ShiftTimeControlsEnabled
        {
            set
            {
                outlookTimePickerStartTimeMax.Enabled = value;
                outlookTimePickerStartTimeMin.Enabled = value;
                outlookTimePickerEndTimeMax.Enabled = value;
                outlookTimePickerEndTimeMin.Enabled = value;
                checkBoxAdvEndTimeMin.Enabled = value;
                checkBoxAdvEndTimeMax.Enabled = value;
                outlookTimePickerWorkTimeMax.Enabled = value;
                outlookTimePickerWorkTimeMin.Enabled = value;
            }
            get { return outlookTimePickerStartTimeMax.Enabled; }
        }

        public bool ActivityEnabled { set { comboBoxAdvActivity.Enabled = value; } get { return comboBoxAdvActivity.Enabled; } }

        public bool ActivityTimeControlsEnabled
        {
            set
            {
                outlookTimePickerActivityStartTimeMax.Enabled = value;
                outlookTimePickerActivityStartTimeMin.Enabled = value;
                outlookTimePickerActivityEndTimeMax.Enabled = value;
                outlookTimePickerActivityEndTimeMin.Enabled = value;
                outlookTimePickerActivityTimeMax.Enabled = value;
                outlookTimePickerActivityTimeMin.Enabled = value;
            }
            get { return outlookTimePickerActivityStartTimeMax.Enabled; }
        }

        public bool SaveButtonEnabled { set { buttonAdvSave.Enabled = value; } get { return buttonAdvSave.Enabled; } }

        public bool EndTimeLimitationMinNextDayEnabled { set { checkBoxAdvEndTimeMin.Enabled = value; } get { return checkBoxAdvEndTimeMin.Enabled; } }

        public bool EndTimeLimitationMaxNextDayEnabled { set { checkBoxAdvEndTimeMax.Enabled = value; } get { return checkBoxAdvEndTimeMax.Enabled; } }

        public ShiftCategory ShiftCategory
        {
            set
            {
                if (value != null && value.Id != null)
                {
                    comboBoxAdvShiftCategory.SelectedValue = value.Id;
                }
                else
                {
                    if (comboBoxAdvShiftCategory.Items.Count > 0)
                    comboBoxAdvShiftCategory.SelectedIndex = 0;
                }
            }
            get { return (ShiftCategory) comboBoxAdvShiftCategory.SelectedItem; }
        }

        public DayOff DayOff
        {
            set
            {
                if (value != null && value.Id != null)
                {
                    comboBoxAdvDayOff.SelectedValue = value.Id;
                }
                else
                {
                    if (comboBoxAdvDayOff.Items.Count>0)
                    comboBoxAdvDayOff.SelectedIndex = 0;
                }
            }
            get { return (DayOff) comboBoxAdvDayOff.SelectedItem; }
        }

        public Absence Absence
        {
            set
            {
                if(value != null && value.Id != null)
                {
                    comboBoxAdvAbsence.SelectedValue = value.Id;
                }
                else
                {
                    if (comboBoxAdvAbsence.Items.Count > 0)
                        comboBoxAdvAbsence.SelectedIndex = 0;
                }
            }
            get { return (Absence) comboBoxAdvAbsence.SelectedItem; }
        }

        public TimeSpan? StartTimeLimitationMin { set { outlookTimePickerStartTimeMin.SetTimeValue(value); } get { return outlookTimePickerStartTimeMin.TimeValue(); } }

        public TimeSpan? StartTimeLimitationMax { set { outlookTimePickerStartTimeMax.SetTimeValue(value); } get { return outlookTimePickerStartTimeMax.TimeValue(); } }

        public TimeSpan? EndTimeLimitationMin { set { outlookTimePickerEndTimeMin.SetTimeValue(value); } get { return outlookTimePickerEndTimeMin.TimeValue(); } }

        public TimeSpan? EndTimeLimitationMax { set { outlookTimePickerEndTimeMax.SetTimeValue(value); } get { return outlookTimePickerEndTimeMax.TimeValue(); } }

        public bool EndTimeLimitationMinNextDay { set { checkBoxAdvEndTimeMin.Checked = value; } get { return checkBoxAdvEndTimeMin.Checked; } }

        public bool EndTimeLimitationMaxNextDay { set { checkBoxAdvEndTimeMax.Checked = value; } get { return checkBoxAdvEndTimeMax.Checked; } }

        public TimeSpan? WorkTimeLimitationMin { set { outlookTimePickerWorkTimeMin.SetTimeValue(value); } get { return outlookTimePickerWorkTimeMin.TimeValue(); } }

        public TimeSpan? WorkTimeLimitationMax { set { outlookTimePickerWorkTimeMax.SetTimeValue(value); } get { return outlookTimePickerWorkTimeMax.TimeValue(); } }

        public Activity Activity
        {
            set
            {
                if (value != null && value.Id != null)
                {
                    comboBoxAdvActivity.SelectedValue = value.Id;
                }
                else
                {
                    if (comboBoxAdvActivity.Items.Count>0)
                    comboBoxAdvActivity.SelectedIndex = 0;
                }
            }
            get { return (Activity) comboBoxAdvActivity.SelectedItem; }
        }

        public TimeSpan? ActivityEndTimeLimitationMin { set { outlookTimePickerActivityEndTimeMin.SetTimeValue(value); } get { return outlookTimePickerActivityEndTimeMin.TimeValue(); } }

        public TimeSpan? ActivityEndTimeLimitationMax { set { outlookTimePickerActivityEndTimeMax.SetTimeValue(value); } get { return outlookTimePickerActivityEndTimeMax.TimeValue(); } }

        public TimeSpan? ActivityStartTimeLimitationMin { set { outlookTimePickerActivityStartTimeMin.SetTimeValue(value); } get { return outlookTimePickerActivityStartTimeMin.TimeValue(); } }

        public TimeSpan? ActivityStartTimeLimitationMax { set { outlookTimePickerActivityStartTimeMax.SetTimeValue(value); } get { return outlookTimePickerActivityStartTimeMax.TimeValue(); } }

        public TimeSpan? ActivityTimeLimitationMin { set { outlookTimePickerActivityTimeMin.SetTimeValue(value); } get { return outlookTimePickerActivityTimeMin.TimeValue(); } }

        public TimeSpan? ActivityTimeLimitationMax { set { outlookTimePickerActivityTimeMax.SetTimeValue(value); } get { return outlookTimePickerActivityTimeMax.TimeValue(); } }

        public string StartTimeLimitationErrorMessage
        {
            set
            {
                SetLimitationErrorMessage(value, labelStartTimeValidationError, labelStartTime, outlookTimePickerStartTimeMin,
                                          outlookTimePickerStartTimeMax);
            }
            get { return toolTipValidation.GetToolTip(labelStartTimeValidationError); }
        }

        public string EndTimeLimitationErrorMessage { set { SetLimitationErrorMessage(value, labelEndTimeValidationError, labelEndTime, outlookTimePickerEndTimeMin, outlookTimePickerEndTimeMax); } get { return toolTipValidation.GetToolTip(labelEndTimeValidationError); } }

        public string WorkTimeLimitationErrorMessage
        {
            set
            {
                SetLimitationErrorMessage(value, labelWorkTimeValidationError, labelWorkTime, outlookTimePickerWorkTimeMin,
                                          outlookTimePickerWorkTimeMax);
            }
            get { return toolTipValidation.GetToolTip(labelWorkTimeValidationError); }
        }

        public string ActivityStartTimeLimitationErrorMessage
        {
            set
            {
                SetLimitationErrorMessage(value, labelActivityStartTimeValidationError, labelActivityStartTime, outlookTimePickerActivityStartTimeMin,
                                          outlookTimePickerActivityStartTimeMax);
            }
            get { return toolTipValidation.GetToolTip(labelActivityStartTimeValidationError); }
        }

        public string ActivityEndTimeLimitationErrorMessage
        {
            set
            {
                SetLimitationErrorMessage(value, labelActivityEndTimeValidationError, labelActivityEndTime, outlookTimePickerActivityEndTimeMin,
                                          outlookTimePickerActivityEndTimeMax);
            }
            get { return toolTipValidation.GetToolTip(labelActivityEndTimeValidationError); }
        }

        public string ActivityTimeLimitationErrorMessage
        {
            set
            {
                SetLimitationErrorMessage(value, labelActivityTimeValidationError, labelActivityTime, outlookTimePickerActivityTimeMin,
                                          outlookTimePickerActivityTimeMax);
            }
            get { return toolTipValidation.GetToolTip(labelActivityTimeValidationError); }
        }

        private void SetLimitationErrorMessage(string value, Label labelValidationError, Label label, OutlookTimePicker timeMin,
                                               OutlookTimePicker timeMax)
        {
            toolTipValidation.SetToolTip(labelValidationError, value);
            toolTipValidation.SetToolTip(label, value);
            toolTipValidation.SetToolTip(timeMin, value);
            toolTipValidation.SetToolTip(timeMax, value);
            if (value == null)
            {
                labelValidationError.Visible = false;
                label.Font = new Font(label.Font, FontStyle.Regular);
                label.ForeColor = Color.Black;
            }
            else
            {
                labelValidationError.Visible = true;
                label.Font = new Font(label.Font, FontStyle.Bold);
                label.ForeColor = Color.Red;
            }
        }

        #endregion

        private void comboBoxAdvShiftCategory_Click(object sender, EventArgs e)
        {

        }
    }
}