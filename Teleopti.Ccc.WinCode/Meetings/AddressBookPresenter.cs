using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings
{
    public class AddressBookPresenter
    {
        private readonly IAddressBookView _view;
        private readonly AddressBookViewModel _model;
        private DateOnly _startDate;
        private string _functionPath;
        
        public AddressBookPresenter(IAddressBookView view, AddressBookViewModel model, DateOnly startDate)
        {
            _view = view;
            _model = model;
            _startDate = startDate;
        }

        public void Initialize()
        {
            _functionPath = DefinedRaptorApplicationFunctionPaths.ModifyMeetings;

            _view.SetCurrentDate(_startDate);
            _view.SetRequiredParticipants(_model.RequiredParticipants);
            _view.SetOptionalParticipants(_model.OptionalParticipants);
            
            checkPermissionsOnPeople();

            _view.PrepareGridView(_model.PersonModels);
        }

        public AddressBookViewModel AddressBookViewModel
        {
            get { return _model; }
        }

        private void checkPermissionsOnPeople()
        {
            for (int index = _model.PersonModels.Count - 1; index > -1; index--)
            {
                ContactPersonViewModel contactPersonView = _model.PersonModels[index];
                bool isPermitted = contactPersonView.FilterByPermission(_functionPath, _startDate);

                if (!isPermitted) _model.PersonModels.RemoveAt(index);
            }
        }

        public void SetCurrentDate(DateOnly startDate)
        {
            _startDate = startDate;
            foreach (ContactPersonViewModel viewModel in _model.PersonModels)
            {
                viewModel.CurrentDate = _startDate;
            }
            _view.PerformSearch();
        }

        public bool IsVisible(ContactPersonViewModel contactPersonViewModel)
        {
            bool filterOut = contactPersonViewModel.CurrentPeriod != null;
            if (filterOut) filterOut = contactPersonViewModel.FilterByValue(_view.GetSearchCriteria());
            if (filterOut) filterOut = contactPersonViewModel.FilterByPermission(_functionPath, _startDate);

            return filterOut;
        }

        public void AddOptionalParticipants(IEnumerable<ContactPersonViewModel> contactPersonViewModels)
        {
            foreach (ContactPersonViewModel viewModel in contactPersonViewModels)
            {
                if(viewModel != null)
                    _model.AddOptionalParticipant(viewModel);
            }
            _view.SetOptionalParticipants(_model.OptionalParticipants);
        }

        public void AddRequiredParticipants(IEnumerable<ContactPersonViewModel> contactPersonViewModels)
        {
            foreach (ContactPersonViewModel viewModel in contactPersonViewModels)
            {
                if(viewModel != null)
                    _model.AddRequiredParticipant(viewModel);
            }
            _view.SetRequiredParticipants(_model.RequiredParticipants);
        }

        public void ParseRequiredParticipants(string participantText)
        {
            var participantsFromText = participantText.Split(new[] { MeetingViewModel.ParticipantSeparator },
                                                 StringSplitOptions.RemoveEmptyEntries);

            _model.RequiredParticipantList.Clear();
            foreach (var foundName in participantsFromText.Select(s => _model.PersonModels.First(conP => conP.FullName == s)).Where(foundName => foundName != null))
                _model.RequiredParticipantList.Add(foundName);

            _view.SetRequiredParticipants(_model.RequiredParticipants);
        }

        public void ParseOptionalParticipants(string participantText)
        {
            var participantsFromText = participantText.Split(new[] { MeetingViewModel.ParticipantSeparator },
                                                 StringSplitOptions.RemoveEmptyEntries);

            _model.OptionalParticipantList.Clear();
            foreach (var foundName in participantsFromText.Select(s => _model.PersonModels.First(conP => conP.FullName == s)).Where(foundName => foundName != null))
                _model.OptionalParticipantList.Add(foundName);

            _view.SetOptionalParticipants(_model.OptionalParticipants);
        }
    }
}