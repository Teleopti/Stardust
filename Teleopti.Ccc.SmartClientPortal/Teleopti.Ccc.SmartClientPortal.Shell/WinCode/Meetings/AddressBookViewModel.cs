using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.WinCode.Meetings
{
    public class AddressBookViewModel
    {
        private readonly IList<ContactPersonViewModel> _personModels;
        private readonly IList<ContactPersonViewModel> _requiredPersonModels;
        private readonly IList<ContactPersonViewModel> _optionalPersonModels;

        public AddressBookViewModel(IList<ContactPersonViewModel> personModels, IEnumerable<ContactPersonViewModel> requiredPersonModels, IEnumerable<ContactPersonViewModel> optionalPersonModels)
        {
            _personModels = personModels;
            _requiredPersonModels = new List<ContactPersonViewModel>(requiredPersonModels);
            _optionalPersonModels = new List<ContactPersonViewModel>(optionalPersonModels);
        }

        public IList<ContactPersonViewModel> PersonModels
        {
            get { return _personModels; }
        }

        public string RequiredParticipants
        {
            get { return ParticipantsFromModel(_requiredPersonModels); }
        }

        public string OptionalParticipants
        {
            get { return ParticipantsFromModel(_optionalPersonModels); }
        }

        public IList<ContactPersonViewModel> RequiredParticipantList
        {
            get { return _requiredPersonModels; }
        }

        public IList<ContactPersonViewModel> OptionalParticipantList
        {
            get { return _optionalPersonModels; }
        }

        private static string ParticipantsFromModel(IEnumerable<ContactPersonViewModel> personViewModels)
        {
            return string.Join(MeetingViewModel.ParticipantSeparator, personViewModels.Select(p => p.FullName).ToArray());
        }

        public void AddRequiredParticipant(ContactPersonViewModel contactPersonViewModel)
        {
	        if (_requiredPersonModels.Any(requiredPersonModel => requiredPersonModel.Id == contactPersonViewModel.Id)) return;
	        _requiredPersonModels.Add(contactPersonViewModel);
        }

	    public void AddOptionalParticipant(ContactPersonViewModel contactPersonViewModel)
        {
			if (_optionalPersonModels.Any(optionalPersonModel => optionalPersonModel.Id == contactPersonViewModel.Id)) return;
            _optionalPersonModels.Add(contactPersonViewModel);
        }
    }
}