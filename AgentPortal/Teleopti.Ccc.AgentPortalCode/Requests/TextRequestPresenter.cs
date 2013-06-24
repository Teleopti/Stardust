using System;
using System.ServiceModel;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Requests
{
    public class TextRequestPresenter
    {
        private readonly ITextRequestView _view;
        private PersonRequestDto _model;
        private readonly ITeleoptiSchedulingService _teleoptiSchedulingService;

        public TextRequestPresenter(ITextRequestView view, PersonRequestDto model, ITeleoptiSchedulingService teleoptiSchedulingService)
        {
            _view = view;
            _model = model;
            _teleoptiSchedulingService = teleoptiSchedulingService;
        }

        public TextRequestPresenter(ITextRequestView view, PersonDto personDto, DateTimePeriodDto dateTimePeriodDto, ITeleoptiSchedulingService teleoptiSchedulingService)
        {
            _view = view;
            _teleoptiSchedulingService = teleoptiSchedulingService;
            CreateModel(personDto, dateTimePeriodDto);
        }

        public PersonRequestDto PersonRequest
        {
            get { return _model; }
        }

        private void CreateModel(PersonDto person, DateTimePeriodDto dateTimePeriod)
        {
            _model = new PersonRequestDto {Person = person, CreatedDate = DateTime.Now, CanDelete = true};

            TextRequestDto textRequest;
            if (_model.Request == null)
            {
                textRequest = new TextRequestDto();
                _model.Request = textRequest;
            }
            else
            {
                textRequest = _model.Request as TextRequestDto;
            }

            if (textRequest == null) return;

            textRequest.Period = dateTimePeriod;
        }

        public void Initialize()
        {
            DateTime startDateTime = _model.Request.Period.LocalStartDateTime;
            DateTime endDateTime = _model.Request.Period.LocalEndDateTime;
            _view.InitializeDateTimePickers(startDateTime, endDateTime);
            _view.Subject = _model.Subject;
            _view.Message = _model.Message;
            _view.RequestDate = _model.CreatedDate;
            _view.DenyReason = LanguageResourceHelper.Translate(_model.DenyReason);
            _view.Status = LanguageResourceHelper.TranslateEnumValue(_model.RequestStatus);
            _view.RequestDate = _model.CreatedDate; 

            var textRequest = _model.Request as TextRequestDto;
            
            if (textRequest != null)
            {
                _view.DeleteButtonEnabled = _model.CanDelete;
            }

            if (string.IsNullOrEmpty(_model.DenyReason))
            {
                _view.SetDenyReasonVisible(false);
            }

            if (_model.RequestStatus == RequestStatusDto.Approved 
				|| _model.RequestStatus == RequestStatusDto.Denied)
                _view.SetFormReadOnly(true);
        }

        public bool Delete()
        {
			if (!_model.Id.HasValue) return true;

        	try
        	{
				_teleoptiSchedulingService.DeletePersonRequest(_model);
        	}
        	catch (FaultException exception)
        	{
        		_view.ShowDeleteErrorMessage(exception.Message);
        		return false;
        	}

        	return true;
        }

        public void Save()
        {
            _model.Subject = _view.Subject;
            _model.Message = _view.Message;
            _model.Request.Period.LocalStartDateTime = _view.StartDateTime;
            _model.Request.Period.LocalEndDateTime = _view.EndDateTime;
            _view.FixUtcDateTimes();

            if (IsTextRequestValid())
                _teleoptiSchedulingService.SavePersonRequest(_model);
        }

        public bool IsTextRequestValid()
        {
            if (string.IsNullOrEmpty(_model.Subject))
                throw new ArgumentException(UserTexts.Resources.PersonRequestEmptySubjectError);

            var request = _model.Request as TextRequestDto;

            if (request != null)
            {
                if (_model.Request.Period.LocalStartDateTime.CompareTo(_model.Request.Period.LocalEndDateTime) >= 0)
                    throw new ArgumentException(UserTexts.Resources.DateFromGreaterThanDateTo);
            }
            return true;
        }
    }
}
