using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public class AddressBookPresenter
    {
        private readonly IAddressBookView _view;
        private readonly AddressBookViewModel _model;
        private DateOnly _startDate;
		private string _functionPath;
		private IList<ContactPersonViewModel> _originalPersonModels;
        
        public AddressBookPresenter(IAddressBookView view, AddressBookViewModel model, DateOnly startDate)
        {
            _view = view;
            _model = model;
            _startDate = startDate;
        }

        public void Initialize()
		{
			_originalPersonModels = new List<ContactPersonViewModel>();
			foreach (var personModel in _model.PersonModels)
			{
				_originalPersonModels.Add(personModel);
			}
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

		public void RemoveIndexesRequired(IList<int> removedIndexes)
		{
			foreach (var removedIndex in removedIndexes.Reverse())
			{
				if(_model.RequiredParticipantList.Count > removedIndex)
					_model.RequiredParticipantList.RemoveAt(removedIndex);
			}

			_view.SetRequiredParticipants(_model.RequiredParticipants);
		}

		public void RemoveIndexesOptional(IList<int> removedIndexes)
		{
			foreach (var removedIndex in removedIndexes.Reverse())
			{
				if(_model.OptionalParticipantList.Count > removedIndex)
					_model.OptionalParticipantList.RemoveAt(removedIndex);
			}

			_view.SetOptionalParticipants(_model.OptionalParticipants);
		}
    }
}