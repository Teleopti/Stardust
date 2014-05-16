using System;
using System.Data;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using System.Web.Services.Protocols;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.Requests;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using System.Windows.Forms;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Requests
{
    /// <summary>
    /// Represent GUI manipulation functionality of Absence Request
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-09-18
    /// </remarks>
    public partial class AbsenceRequestView : BaseRibbonForm, IAbsenceRequestView
    {
        private RequestStateHolder _requestStateHolder;
        private readonly AbsenceRequestPresenter _presenter;
        private bool _initializing;


        private void AbsenceRequestView_Load(object sender, EventArgs e)
        {
            _presenter.InitializeView();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequestView"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-01-15
        /// </remarks>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        private AbsenceRequestView()
        {
            InitializeComponent();
            InitializeAbsenceComboBox();
            if (!DesignMode)
            {
                SetTexts();
                tableLayoutPanelMain.BackColor = UserTexts.ThemeSettings.Default.StandardOfficeFormBackground;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequestView"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        public AbsenceRequestView(DateTimePeriodDto dateTimePeriod) : this()
        {
            _presenter = new AbsenceRequestPresenter(SdkServiceHelper.SchedulingService, StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson, this, dateTimePeriod);
            InitializeControl(new RequestStateHolder());
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequestView"/> class.
        /// </summary>
        /// <param name="personRequest">The person request.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        public AbsenceRequestView(PersonRequestDto personRequest) : this()
        {
            _presenter = new AbsenceRequestPresenter(SdkServiceHelper.SchedulingService, this, personRequest);
            InitializeControl(new RequestStateHolder());
        }


        /// <summary>
        /// Initializes the control.
        /// </summary>
        /// <param name="requestStateHolder">The request state holder.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-10
        /// </remarks>
        private void InitializeControl(RequestStateHolder requestStateHolder)
        {
            _requestStateHolder = requestStateHolder;

            office2007OutlookTimePickerAPStartTime.TimeIntervalInDropDown = _requestStateHolder.Resolution;
            office2007OutlookTimePickerAPEndTime.TimeIntervalInDropDown = _requestStateHolder.Resolution;

			dateTimePickerAdvStart.Culture = CultureInfo.CurrentCulture;
			dateTimePickerAdvEnd.Culture = CultureInfo.CurrentCulture;

            autoLabelRequestDateValue.Text = DateTime.Now.ToShortDateString();

            dateTimePickerAdvStart.Focus();
        }


        /// <summary>
        /// Initializes the absence combo box.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-25
        /// </remarks>
        private void InitializeAbsenceComboBox()
        {
            comboBoxAdvAbsence.DisplayMember = "Name";
            comboBoxAdvAbsence.ValueMember = "Id";
            comboBoxAdvAbsence.DataSource = RequestStateHolder.LoadRequestableAbsence();
        }


        /// <summary>
        /// Handles the CheckStateChanged event of the checkBoxAdvAllDay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/12/2008
        /// </remarks>
        private void checkBoxAdvAllDay_CheckStateChanged(object sender, EventArgs e)
        {
            _presenter.ChangeAllDayState(checkBoxAdvAllDay.CheckState);
        }

        public TimePeriod TimePickerTimePeriod
        {
            get
            {
                TimeSpan start = office2007OutlookTimePickerAPStartTime.TimeValue();
                TimeSpan end = office2007OutlookTimePickerAPEndTime.TimeValue();

                return new TimePeriod(start, end);
            }
            set
            {
                office2007OutlookTimePickerAPEndTime.SetTimeValue(value.EndTime);
                office2007OutlookTimePickerAPStartTime.SetTimeValue(value.StartTime);
            }
        }

        public bool TimePickersEnabled
        {
            get { return office2007OutlookTimePickerAPStartTime.Enabled;}
            set
            {
                office2007OutlookTimePickerAPStartTime.Enabled = value;
                office2007OutlookTimePickerAPEndTime.Enabled = value;

            }
        }

        public string Subject
        {
            get { return textBoxExtSubject.Text; }
            set { textBoxExtSubject.Text = value; }
        }

        public DateTime RequestDate
        {
            get { return DateTime.Parse(autoLabelRequestDateValue.Text, Thread.CurrentThread.CurrentCulture); }
            set { autoLabelRequestDateValue.Text = value.ToShortDateString(); }
        }

        public string Status
        {
            get { return autoLabelRequestStatus.Text; }
            set { autoLabelRequestStatus.Text = value; }
        }

        public string Message
        {
            get { return textBoxExtMessage.Text; }
            set { textBoxExtMessage.Text = value; }
        }

        public AbsenceDto AbsenceType
        {
            get { return comboBoxAdvAbsence.SelectedItem as AbsenceDto; }
            set
            {
                foreach (AbsenceDto absenceDto in comboBoxAdvAbsence.Items)
                {
                    if (absenceDto.Id.Equals(value.Id))
                    {
                        comboBoxAdvAbsence.SelectedItem = absenceDto;
                    }
                }
            }
        }

        public DateTime SelectedStartDateTime
        {
            get 
            {
                if(IsValidTime(office2007OutlookTimePickerAPStartTime.Text))
                    return dateTimePickerAdvStart.Value.Date.Add(office2007OutlookTimePickerAPStartTime.TimeValue());
                else
                    return _presenter.PersonRequest.Request.Period.LocalStartDateTime;
            }
        }

        public DateTime SelectedEndDateTime
        {
            get 
            { 
                if(IsValidTime(office2007OutlookTimePickerAPEndTime.Text))
                    return dateTimePickerAdvEnd.Value.Date.Add(office2007OutlookTimePickerAPEndTime.TimeValue());
                else
                    return _presenter.PersonRequest.Request.Period.LocalEndDateTime;
            }
        }

        public void SetDateTimePickers(DateTime startTime, DateTime endTime)
        {
            dateTimePickerAdvStart.Value = startTime;
            dateTimePickerAdvEnd.Value = endTime;

            office2007OutlookTimePickerAPStartTime.SetTimeValue(startTime.TimeOfDay);
            office2007OutlookTimePickerAPEndTime.SetTimeValue(endTime.TimeOfDay);
        }

        public void InitializeDateTimePickers(DateTime startTime, DateTime endTime)
        {
            _initializing = true;

            _presenter.PersonRequest.Request.Period.LocalStartDateTime = startTime;
            _presenter.PersonRequest.Request.Period.LocalEndDateTime = endTime;

            office2007OutlookTimePickerAPStartTime.CreateAndBindList();
            office2007OutlookTimePickerAPEndTime.CreateAndBindList();

            office2007OutlookTimePickerAPStartTime.SetTimeValue(startTime.TimeOfDay);
            office2007OutlookTimePickerAPEndTime.SetTimeValue(endTime.TimeOfDay);

            dateTimePickerAdvStart.Value = startTime;
            dateTimePickerAdvEnd.Value = endTime;

            _initializing = false;
        }

        public bool IsAllDay
        {
            get 
            {
                if (checkBoxAdvAllDay.CheckState == CheckState.Checked)
                {
                    return true;
                }
                return false;
            }
            set
            {
                if (value)
                {
                    checkBoxAdvAllDay.CheckState = CheckState.Checked;
                }
                else
                {
                    checkBoxAdvAllDay.CheckState = CheckState.Unchecked;
                }
            }
        }

        public bool DeleteButtonEnabled
        {
            get { return toolStripButtonDelete.Enabled; }
            set { toolStripButtonDelete.Enabled = value; }
        }

        public string DenyReason
        {
            get { return labelDenyReason.Text; }
            set { labelDenyReason.Text = value; }
        }

        public void SetDenyReasonVisible(bool value)
        {
            gradientPanelScheduleHasChanged.Visible = value;
        }

        public void SetFormReadOnly(bool value)
        {
            textBoxExtSubject.ReadOnly = value;
            textBoxExtMessage.ReadOnly = value;

            officeButtonSaveAndClose.Enabled = !value;
            toolStripButtonSaveAndClose.Enabled = !value;
            toolStripButtonDelete.Enabled = !value;

            comboBoxAdvAbsence.ReadOnly = value;

            checkBoxAdvAllDay.ReadOnlyMode = value;

            dateTimePickerAdvStart.ReadOnly = value;
            dateTimePickerAdvEnd.ReadOnly = value;

            office2007OutlookTimePickerAPStartTime.ReadOnly = value;
            office2007OutlookTimePickerAPEndTime.ReadOnly = value;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortal.Helper.MessageBoxHelper.ShowErrorMessage(System.String,System.String)")]
		public void ShowDeleteErrorMessage(string message)
    	{
    		MessageBoxHelper.ShowErrorMessage(
    			string.Concat(UserTexts.Resources.PleaseTryAgainLater, Environment.NewLine, Environment.NewLine,
    			              "Error information: ", message), UserTexts.Resources.AgentPortal);
    	}

    	private void toolStripButtonSaveAndClose_Click(object sender, EventArgs e)
        {
            save();
        }

        private void officeButtonSaveAndClose_Click(object sender, EventArgs e)
        {
            save();
        }

        private void save()
        {
            // Trigger the leave event for timepickers, not so nice. Validation/handling of date and time should be the same in both text and absence request!
			//if (office2007OutlookTimePickerAPStartTime.Focused || office2007OutlookTimePickerAPEndTime.Focused)
			//    textBoxExtSubject.Focus();

			UpdateStartTime();
			UpdateEndTime();

            FixUtcDateTimes();

            try
            {
                ValidateTimeValues();
                _presenter.Save();
                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBoxHelper.ShowErrorMessage(ex.Message, UserTexts.Resources.AgentPortal);
            }
            catch (DataException ex)
            {
                string dataErrorMessage = string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.ErrorOccuredWhenAccessingTheDataSource + ".\n\nError information: {0}", ex.Message);
                MessageBoxHelper.ShowErrorMessage(dataErrorMessage, UserTexts.Resources.AgentPortal);
            }
            catch (FaultException ex)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.SaveError + "\n\nError information: {0}", ex.Message);
                MessageBoxHelper.ShowErrorMessage(errorMessage, UserTexts.Resources.AgentPortal);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


        /// <summary>
        /// Fixes the UTC date times.
        /// TODO: Fix this
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-17
        /// </remarks>
        private void FixUtcDateTimes()
        {
            DateTimePeriodDto dateTimePeriodDto = _presenter.PersonRequest.Request.Period;
            var timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId);
            dateTimePeriodDto.UtcStartTime = TimeZoneHelper.ConvertToUtc(dateTimePeriodDto.LocalStartDateTime, timeZone);
            dateTimePeriodDto.UtcEndTime = TimeZoneHelper.ConvertToUtc(dateTimePeriodDto.LocalEndDateTime, timeZone);
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBoxHelper.ShowConfirmationMessage(
                UserTexts.Resources.AreYouSureYouWantToDelete,
                UserTexts.Resources.AgentPortal);

            if (result != DialogResult.Yes) return;
            _presenter.Delete();
            Close();
        }

        private void textBoxExtSubject_TextChanged(object sender, EventArgs e)
        {
            _presenter.Subject = textBoxExtSubject.Text;
        }

        private void textBoxExtMessage_TextChanged(object sender, EventArgs e)
        {
            _presenter.Message = textBoxExtMessage.Text;
        }


        private void comboBoxAdvAbsence_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_presenter != null)
            {
                AbsenceDto absenceDto = (AbsenceDto)comboBoxAdvAbsence.SelectedItem;
                _presenter.SetAbsenceType(absenceDto);
            }

        }

        private void dateTimePickerAdvStart_ValueChanged(object sender, EventArgs e)
        {
            if (_presenter != null)
            {
                DateTime previousValue = _presenter.PersonRequest.Request.Period.LocalStartDateTime;
                DateTime newValue = dateTimePickerAdvStart.Value.Date.Add(office2007OutlookTimePickerAPStartTime.TimeValue());

                _presenter.SetStartDateTime(previousValue, newValue);
            }
        }

        private void dateTimePickerAdvEnd_ValueChanged(object sender, EventArgs e)
        {
            if (_presenter != null)
            {
                DateTime previousValue = _presenter.PersonRequest.Request.Period.LocalEndDateTime;
                DateTime newValue = dateTimePickerAdvEnd.Value.Date.Add(office2007OutlookTimePickerAPEndTime.TimeValue());

                _presenter.SetEndDateTime(previousValue, newValue);
            }
        }


        private void ValidateTimeValues()
        {
            if(!((IsValidTime(office2007OutlookTimePickerAPStartTime.Text) && IsValidTime(office2007OutlookTimePickerAPEndTime.Text))))
                throw new ArgumentException(UserTexts.Resources.MustSpecifyValidTime);

            if(!(dateTimePickerAdvStart.Value.Date.Add(office2007OutlookTimePickerAPStartTime.TimeValue()) < 
                dateTimePickerAdvEnd.Value.Date.Add(office2007OutlookTimePickerAPEndTime.TimeValue())))
                throw new ArgumentException(UserTexts.Resources.EndTimeMustBeGreaterOrEqualToStartTime);
        }

        private static bool IsValidTime(string timeString)
        {
            DateTime time;
            if (DateTime.TryParse(timeString, out time))
            {
                if (time.TimeOfDay.TotalHours >= 0d && time.TimeOfDay.TotalMinutes < 1440)
                    return true;
            }
            return false;
        }


        private void office2007OutlookTimePickerAPStartTime_Leave(object sender, EventArgs e)
        {
			UpdateStartTime();
        }

        private void office2007OutlookTimePickerAPEndTime_Leave(object sender, EventArgs e)
        {
			UpdateEndTime();
        }

		private void UpdateEndTime()
		{
			if (_presenter == null || _initializing) return;

			var previousValue = _presenter.PersonRequest.Request.Period.LocalEndDateTime;

			if (IsValidTime(office2007OutlookTimePickerAPEndTime.Text))
			{
				var newValue = dateTimePickerAdvEnd.Value.Date.Add(office2007OutlookTimePickerAPEndTime.TimeValue());

				if (!previousValue.Equals(newValue))
				{
					_presenter.SetEndDateTime(previousValue, newValue);
				}
			}
			else
			{
				office2007OutlookTimePickerAPEndTime.SetTimeValue(previousValue.TimeOfDay);
			}
		}

		private void UpdateStartTime()
		{
			if (_presenter == null || _initializing) return;

			var previousValue = _presenter.PersonRequest.Request.Period.LocalStartDateTime;

			if (IsValidTime(office2007OutlookTimePickerAPStartTime.Text))
			{
				var newValue = dateTimePickerAdvStart.Value.Date.Add(office2007OutlookTimePickerAPStartTime.TimeValue());

				if (!previousValue.Equals(newValue))
				{
					_presenter.SetStartDateTime(previousValue, newValue);
				}
			}
			else
			{
				office2007OutlookTimePickerAPStartTime.SetTimeValue(previousValue.TimeOfDay);
			}
		}

    }
}
