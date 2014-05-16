using System;
using System.Data;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Requests;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Requests
{
    public partial class TextRequestView : BaseRibbonForm, ITextRequestView
    {
        private readonly TextRequestPresenter _presenter;

        public TextRequestView()
        {
            InitializeComponent();
            SetTexts();
        }
        public TextRequestView(PersonRequestDto model, ITeleoptiSchedulingService service) : this()
        {
            _presenter = new TextRequestPresenter(this, model, service);
        }

        public TextRequestView(DateTimePeriodDto period, PersonDto person, ITeleoptiSchedulingService service) : this()
        {
            _presenter = new TextRequestPresenter(this, person , period, service);
        }

        public void InitializeDateTimePickers(DateTime startDateTime, DateTime endDateTime)
        {
            office2007OutlookTimePickerAPStartTime.CreateAndBindList();
            office2007OutlookTimePickerAPEndTime.CreateAndBindList();

            office2007OutlookTimePickerAPStartTime.SetTimeValue(startDateTime.TimeOfDay);
            office2007OutlookTimePickerAPEndTime.SetTimeValue(endDateTime.TimeOfDay);

            dateTimePickerAdvStart.Value = startDateTime;
            dateTimePickerAdvEnd.Value = endDateTime;
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

        public string DenyReason
        {
            get { return labelDenyReason.Text; }
            set { labelDenyReason.Text = value; }
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

        public bool DeleteButtonEnabled
        {
            get { return toolStripButtonDelete.Enabled; }
            set { toolStripButtonDelete.Enabled = value; }
        }

        private void TextRequestView_Load(object sender, EventArgs e)
        {
			dateTimePickerAdvStart.Culture = CultureInfo.CurrentCulture;
			dateTimePickerAdvEnd.Culture = CultureInfo.CurrentCulture;

            _presenter.Initialize();
        }

        public void SetDenyReasonVisible(bool value)
        {
            gradientPanelScheduleHasChanged.Visible = value;
        }

        public void SetFormReadOnly(bool value)
        {
            textBoxExtSubject.ReadOnly = value;
            textBoxExtMessage.ReadOnly = value;
            toolStripButtonSaveAndClose.Enabled = !value;
            toolStripButtonDelete.Enabled = !value;
            
            dateTimePickerAdvStart.Enabled = value;
            dateTimePickerAdvEnd.Enabled = value;
            office2007OutlookTimePickerAPStartTime.Enabled = value;
            office2007OutlookTimePickerAPEndTime.Enabled = value;
        }

        public DateTime StartDateTime
        {
            get {return dateTimePickerAdvStart.Value.Date.Add(office2007OutlookTimePickerAPStartTime.TimeValue());}
        }

        public DateTime EndDateTime
        {
            get { return dateTimePickerAdvEnd.Value.Date.Add(office2007OutlookTimePickerAPEndTime.TimeValue());}
        }

        private void toolStripButtonSaveAndClose_Click(object sender, EventArgs e)
        {
            save();
        }

        private void save()
        {
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

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBoxHelper.ShowConfirmationMessage(
            UserTexts.Resources.AreYouSureYouWantToDelete,
            UserTexts.Resources.AgentPortal);

            if (result != DialogResult.Yes) return;
            if (_presenter.Delete())
            {
            	Close();
            }
        }


        public void FixUtcDateTimes()
        {
            DateTimePeriodDto dateTimePeriodDto = _presenter.PersonRequest.Request.Period;

            var timeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId);
            dateTimePeriodDto.UtcStartTime = TimeZoneHelper.ConvertToUtc(dateTimePeriodDto.LocalStartDateTime, timeZone);
            dateTimePeriodDto.UtcEndTime = TimeZoneHelper.ConvertToUtc(dateTimePeriodDto.LocalEndDateTime, timeZone);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortal.Helper.MessageBoxHelper.ShowErrorMessage(System.String,System.String)")]
		public void ShowDeleteErrorMessage(string message)
    	{
    		MessageBoxHelper.ShowErrorMessage(
    			string.Concat(UserTexts.Resources.PleaseTryAgainLater, Environment.NewLine, Environment.NewLine,
    			              "Error information: ", message), UserTexts.Resources.AgentPortal);
    	}

    	private void ValidateTimeValues()
        {
            if (!((IsValidTime(office2007OutlookTimePickerAPStartTime.Text) && IsValidTime(office2007OutlookTimePickerAPEndTime.Text))))
                throw new ArgumentException(UserTexts.Resources.MustSpecifyValidTime);

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

		private void toolStripButtonSend_Click(object sender, EventArgs e)
		{
			save();
		}
    }
}
