using System;
using Teleopti.Ccc.WinCode.Meetings;

namespace Teleopti.Ccc.Win.Meetings
{
    /// <summary>
    /// Represents the participant selection event argument of the address book.
    /// </summary>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 8/5/2008
    /// </remarks>
    public class AddressBookParticipantSelectionEventArgs : EventArgs
    {
        private readonly AddressBookViewModel _addressBookViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressBookParticipantSelectionEventArgs"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/5/2008
        /// </remarks>
        protected AddressBookParticipantSelectionEventArgs()
        {
        }

        public AddressBookParticipantSelectionEventArgs(AddressBookViewModel addressBookViewModel) : this()
        {
            _addressBookViewModel = addressBookViewModel;
        }

        public AddressBookViewModel AddressBookViewModel
        {
            get { return _addressBookViewModel; }
        }
    }
}
