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
            foreach (ContactPersonViewModel viewModel in _model.RequiredParticipantList.ToList()) //To avoid modifications to enumerating list
            {
                var foundName = false;
                if (participantsFromText.Contains(viewModel.FullName)) continue;
                for (var j = 0; j < participantsFromText.Length; j++)
                {
                    if (participantsFromText[j].Length <= viewModel.FullName.Trim().Length) continue;
                    var itemEnding = participantsFromText[j].Substring(
                        0, viewModel.FullName.Trim().Length);
                    if (viewModel.FullName.Trim() == itemEnding)
                    {
                        foundName = true;
                    }
                }
                if (!foundName)
                {
                    _model.RequiredParticipantList.Remove(viewModel);
                }
            }
            _view.SetRequiredParticipants(_model.RequiredParticipants);
        }

        public void ParseOptionalParticipants(string participantText)
        {
            var participantsFromText = participantText.Split(new[] { MeetingViewModel.ParticipantSeparator },
                                                 StringSplitOptions.RemoveEmptyEntries);
            foreach (ContactPersonViewModel viewModel in _model.OptionalParticipantList.ToList()) //To avoid modifications to enumerating list
            {
                var foundName = false;
                if (participantsFromText.Contains(viewModel.FullName.Trim())) continue;
                for (var j = 0; j < participantsFromText.Length; j++)
                {
                    if (participantsFromText[j].Length <= viewModel.FullName.Trim().Length) continue;
                    var itemEnding = participantsFromText[j].Substring(
                        0, viewModel.FullName.Trim().Length);
                    if (viewModel.FullName.Trim() == itemEnding)
                    {
                        foundName = true;
                    }
                }
                if (!foundName)
                {
                    _model.OptionalParticipantList.Remove(viewModel);
                } 
            }
            _view.SetOptionalParticipants(_model.OptionalParticipants);
        }
    }
}