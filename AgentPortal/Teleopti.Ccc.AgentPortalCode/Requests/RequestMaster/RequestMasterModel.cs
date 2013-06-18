using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster
{
    public class RequestMasterModel
    {
        private readonly ITeleoptiSchedulingService _sdkService;
        private IList<RequestDetailRow> _dataSource;

        public RequestMasterModel(ITeleoptiSchedulingService sdkService, IList<RequestDetailRow> dataSource)
        {
            _sdkService = sdkService;
            _dataSource = dataSource;
        }

        public RequestMasterModel(ITeleoptiSchedulingService sdkService, PersonDto loggedOnPerson)
        {
            _sdkService = sdkService;
            LoggedOnPerson = loggedOnPerson;
        }

        public PersonDto LoggedOnPerson { get; private set; }

        public ITeleoptiSchedulingService SdkService
        {
            get { return _sdkService; }
        }

        public IList<RequestDetailRow> DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        public string MessageHeader
        {
            get { return UserTexts.Resources.Message; }
        }

        public string RequestDateHeader
        {
            get { return UserTexts.Resources.RequestDate; }
        }
        public string RequestTypeHeader
        {
            get { return UserTexts.Resources.RequestType; }
        }

        public string RequestStatusHeader
        {
            get { return UserTexts.Resources.RequestStatus; }
        }
        public string DetailsHeader
        {
            get { return UserTexts.Resources.Details;}
        }
        public string SubjectHeader
        {
            get { return UserTexts.Resources.Subject;}
        }
        public string LastChangedHeader
        {
            get { return UserTexts.Resources.UpdatedOn; }
        }
    }

    public class RequestDetailRow
    {
        public PersonDto LoggedOnPerson { get; private set; }
        private readonly PersonRequestDto _personRequestDto;

        public RequestDetailRow(PersonRequestDto personRequestDto, PersonDto loggedOnPerson)
        {
            LoggedOnPerson = loggedOnPerson;
            _personRequestDto = personRequestDto;
        }

        public string Message
        {
            get { return _personRequestDto.Message;}
        }

        public string RequestDate
        {
            get { return resolveRequestDate(); }
        }

        public string RequestType
        {
            get { return resolveTypeText(); }
        }

        public StatusDisplay RequestStatus
        {
            get { return resolveStatusText(); }
        }
        
        public string Details
        {
            get { return _personRequestDto.Request.Details; }
        }

        public string Subject
        {
            get{ return _personRequestDto.Subject;}
        }

        public DateTime LastChanged
        {
            get { return _personRequestDto.UpdatedOn; }
        }

        public bool CanDelete
        {
            get { return _personRequestDto.CanDelete; }
        }

        public PersonRequestDto PersonRequest
        {
            get { return _personRequestDto; }
        }

        private string resolveTypeText()
        {
            string type = UserTexts.Resources.RequestTypeShiftTrade;
            if (_personRequestDto.Request is AbsenceRequestDto)
            {
                type = UserTexts.Resources.RequestTypeAbsence;
            }
            else if (_personRequestDto.Request is TextRequestDto)
            {
                type = UserTexts.Resources.RequestTypeText;
            }
            return type;
        }

        private StatusDisplay resolveStatusText()
        {
            var text = LanguageResourceHelper.TranslateEnumValue(_personRequestDto.RequestStatus);

            if (_personRequestDto.RequestStatus == RequestStatusDto.New 
				&& LoggedOnPerson.Id != _personRequestDto.Person.Id)
            {
                return new StatusDisplay(text, _personRequestDto.RequestStatus);
            }

            if (_personRequestDto.RequestStatus != RequestStatusDto.Denied 
				&& _personRequestDto.RequestStatus != RequestStatusDto.Approved
				&& _personRequestDto.RequestStatus != RequestStatusDto.Autodenied)
            {
                var requestDto = _personRequestDto.Request as ShiftTradeRequestDto;
                if (requestDto != null)
                {
                    if (LoggedOnPerson.Id != _personRequestDto.Person.Id && requestDto.ShiftTradeStatus == ShiftTradeStatusDto.OkByMe)
                        text = UserTexts.Resources.WaitingForYourApproval;
                    else
                        text = LanguageResourceHelper.TranslateEnumValue(requestDto.ShiftTradeStatus);
                }
            }
            return new StatusDisplay(text, _personRequestDto.RequestStatus);
        }

        private string resolveRequestDate()
        {
            DateTime localStartDateTime = _personRequestDto.Request.Period.LocalStartDateTime;
            DateTime localEndDateTime = _personRequestDto.Request.Period.LocalEndDateTime; //To overcome issues regarding DateOnly to DateTimePeriod resulting in two dates for one day
            if (localEndDateTime > localStartDateTime)
                localEndDateTime = localEndDateTime.AddSeconds(-1);
            string text = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", localStartDateTime.ToShortDateString(), localEndDateTime.ToShortDateString());
            if (localStartDateTime.Date.Equals(localEndDateTime.Date))
            {
                text = _personRequestDto.RequestedDateLocal.ToShortDateString();
            }
            
            var shiftTradeRequestDto = _personRequestDto.Request as ShiftTradeRequestDto;
            if (shiftTradeRequestDto != null)
            {
                string multiple = UserTexts.Resources.MultipleValuesParanteses;
                if (shiftTradeRequestDto.ShiftTradeSwapDetails.Count > 1)
                {
                    text = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", _personRequestDto.RequestedDateLocal.ToShortDateString(), multiple);
                }
            }
            return text;
        }
    }

    public class StatusDisplay
    {
        public string DisplayText { get; set; }
        public RequestStatusDto RequestStatus { get; set; }

        public StatusDisplay(string displayText, RequestStatusDto staus)
        {
            DisplayText = displayText;
            RequestStatus = staus;
        }
        public override string ToString()
        {
            return DisplayText;
        }
    }
}
