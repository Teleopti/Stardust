using System;
using System.ServiceModel;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Requests
{
    public class AbsenceRequestPresenter
    {
        //private RequestStateHolder _requestStateHolder;
        private ITeleoptiSchedulingService _teleoptiSchedulingService;
        private PersonRequestDto _personRequest;
        private IAbsenceRequestView _absenceRequestView;
        //private TimePeriod _backupTimePeriod;
        private DateTime _backupStartDateTime;
        private DateTime _backupEndDateTime;
        //private DateTime _startDateTime;
        //private DateTime _endDateTime;


        private AbsenceRequestPresenter(ITeleoptiSchedulingService teleoptiSchedulingService, IAbsenceRequestView absenceRequestView)
        {
            _teleoptiSchedulingService = teleoptiSchedulingService;
            _absenceRequestView = absenceRequestView;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequestPresenter"/> class.
        /// Used if request was newly created.
        /// </summary>
        /// <param name="absenceRequestView">The absence request view.</param>
        /// <param name="dateTimePeriodDto">The date time period dto.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-12
        /// </remarks>
        public AbsenceRequestPresenter(ITeleoptiSchedulingService teleoptiSchedulingService, PersonDto personDto, IAbsenceRequestView absenceRequestView, DateTimePeriodDto dateTimePeriodDto) : this(teleoptiSchedulingService, absenceRequestView)
        {
            CreatePersonRequest(personDto, dateTimePeriodDto);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequestPresenter"/> class.
        /// Used if request was loaded.
        /// </summary>
        /// <param name="absenceRequestView">The absence request view.</param>
        /// <param name="personRequestDto">The person request dto.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-12
        /// </remarks>
        public AbsenceRequestPresenter(ITeleoptiSchedulingService teleoptiSchedulingService, IAbsenceRequestView absenceRequestView, PersonRequestDto personRequestDto)
            : this(teleoptiSchedulingService, absenceRequestView)
        {
            _personRequest = personRequestDto;
        }

        public void ChangeAllDayState(CheckState allDayEnabledCheckState)
        {
            if (allDayEnabledCheckState == CheckState.Checked)
            {
                _backupStartDateTime = _absenceRequestView.SelectedStartDateTime;
                _backupEndDateTime = _absenceRequestView.SelectedEndDateTime;
                DateTime startDateTime = _backupStartDateTime.Date;
                DateTime endDateTime = _backupEndDateTime.Date.AddDays(1).AddMinutes(-1);
                _absenceRequestView.SetDateTimePickers(startDateTime, endDateTime);
                _personRequest.Request.Period.LocalStartDateTime = startDateTime;
                _personRequest.Request.Period.LocalEndDateTime = endDateTime;
                _absenceRequestView.TimePickersEnabled = false;
            }
            else
            {
                _absenceRequestView.SetDateTimePickers(_backupStartDateTime, _backupEndDateTime);
                _absenceRequestView.TimePickersEnabled = true;
            }
        }

        public PersonRequestDto PersonRequest
        {
            get {
                return _personRequest;
            }
            set {
                _personRequest = value;
            }
        }

        public DateTime StartDateTime
        {
            get { return _personRequest.Request.Period.LocalStartDateTime; }
        }

        public DateTime EndDateTime
        {
            get { return _personRequest.Request.Period.LocalEndDateTime; }
        }

        public string Subject
        {
            get { return _personRequest.Subject; }
            set { _personRequest.Subject = value; }
        }

        public string Message
        {
            get { return _personRequest.Message; }
            set { _personRequest.Message = value; }
        }




        /// <summary>
        /// Creates the person request.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        private void CreatePersonRequest(PersonDto person, DateTimePeriodDto dateTimePeriodDto)
        {
            _personRequest = new PersonRequestDto();
            _personRequest.Person = person;
            _personRequest.Subject = _absenceRequestView.Subject;
            _personRequest.Message = _absenceRequestView.Message;
            _personRequest.CreatedDate = DateTime.Now;
            _personRequest.CanDelete = true;

            // file Absence Request values
            AbsenceRequestDto absenceRequest;
            if (_personRequest.Request == null)
            {
                absenceRequest = new AbsenceRequestDto();
                _personRequest.Request = absenceRequest;
            }
            else
            {
                absenceRequest = _personRequest.Request as AbsenceRequestDto;
            }

            if (absenceRequest != null)
            {
                absenceRequest.Absence = _absenceRequestView.AbsenceType;
                absenceRequest.Period = dateTimePeriodDto;
            }
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-12
        /// </remarks>
        public void Save()
        {
            if (IsAbsenceRequestValid())
                _teleoptiSchedulingService.SavePersonAbsenceRequest(_personRequest);
        }

        public void SetStartDateTime(DateTime previousDateTime, DateTime changeToDateTime)
        {
            _personRequest.Request.Period.LocalStartDateTime = changeToDateTime;
            if (changeToDateTime > _absenceRequestView.SelectedEndDateTime)
            {
                TimeSpan differenceTimeSpan = _absenceRequestView.SelectedEndDateTime.Subtract(previousDateTime);
                _personRequest.Request.Period.LocalEndDateTime = changeToDateTime.Add(differenceTimeSpan);
            }
            _absenceRequestView.SetDateTimePickers(StartDateTime, EndDateTime);
        }

        public void SetEndDateTime(DateTime previousDateTime, DateTime changeToDateTime)
        {
            _personRequest.Request.Period.LocalEndDateTime = changeToDateTime;
            if (changeToDateTime < _absenceRequestView.SelectedStartDateTime)
            {
                TimeSpan differenceTimeSpan = previousDateTime.Subtract(_absenceRequestView.SelectedStartDateTime);
                _personRequest.Request.Period.LocalStartDateTime = changeToDateTime.Subtract(differenceTimeSpan);
            }
            _absenceRequestView.SetDateTimePickers(StartDateTime, EndDateTime);
        }

        /// <summary>
        /// Initializes the view.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-16
        /// </remarks>
        public void InitializeView()
        {
            DateTime startDateTime = _personRequest.Request.Period.LocalStartDateTime;
            DateTime endDateTime = _personRequest.Request.Period.LocalEndDateTime;
            _absenceRequestView.InitializeDateTimePickers(startDateTime, endDateTime);
            DateTime allDayEndDateTime = startDateTime.AddDays(1).AddMinutes(-1);
            _absenceRequestView.TimePickersEnabled = !(startDateTime.TimeOfDay == TimeSpan.Zero && endDateTime.TimeOfDay == allDayEndDateTime.TimeOfDay);
            _absenceRequestView.IsAllDay = !_absenceRequestView.TimePickersEnabled;
            _absenceRequestView.Subject = _personRequest.Subject;
            _absenceRequestView.Message = _personRequest.Message;
            _absenceRequestView.RequestDate = _personRequest.CreatedDate;
            _absenceRequestView.DenyReason = LanguageResourceHelper.TranslateMessage(_personRequest.DenyReason);
            _absenceRequestView.Status = LanguageResourceHelper.TranslateEnumValue(_personRequest.RequestStatus);
            AbsenceRequestDto absenceRequest = _personRequest.Request as AbsenceRequestDto;
            if (absenceRequest != null)
            {
                _absenceRequestView.AbsenceType = absenceRequest.Absence;
                _absenceRequestView.DeleteButtonEnabled = _personRequest.CanDelete;
            }

            if(string.IsNullOrEmpty(_personRequest.DenyReason))
            {
                _absenceRequestView.SetDenyReasonVisible(false);
            }

            if (_personRequest.RequestStatus == RequestStatusDto.Approved 
				|| _personRequest.RequestStatus == RequestStatusDto.Denied
				|| _personRequest.RequestStatus == RequestStatusDto.Autodenied)
                _absenceRequestView.SetFormReadOnly(true);
        }


        public bool Delete()
        {
			if (!_personRequest.Id.HasValue) return true;

			try
			{
				_teleoptiSchedulingService.DeletePersonRequest(_personRequest);
			}
			catch (FaultException exception)
			{
				_absenceRequestView.ShowDeleteErrorMessage(exception.Message);
				return false;
			}

			return true;
        }

        public void SetAbsenceType(AbsenceDto absenceDto)
        {
            AbsenceRequestDto absenceRequestDto = _personRequest.Request as AbsenceRequestDto;
            if (absenceRequestDto != null)
            {
                absenceRequestDto.Absence = absenceDto;
            }
        }

        public bool IsAbsenceRequestValid()
        {
            if (string.IsNullOrEmpty(_personRequest.Subject))
                throw new ArgumentException(UserTexts.Resources.PersonRequestEmptySubjectError);

            var request = _personRequest.Request as AbsenceRequestDto;

            if (request != null)
            {
                if (_personRequest.Request.Period.UtcStartTime.CompareTo(_personRequest.Request.Period.UtcEndTime) >= 0)
                    throw new ArgumentException(UserTexts.Resources.DateFromGreaterThanDateTo);
                
                if (request.Absence == null)
                {
                    throw new ArgumentException(UserTexts.Resources.AbsenceRequestEmptyAbsenceTypeError);
                }
            }
            return true;
        }
    }
}