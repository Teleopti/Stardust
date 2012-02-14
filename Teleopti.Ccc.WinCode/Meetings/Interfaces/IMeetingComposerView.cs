using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Interfaces
{
    public interface IMeetingComposerView : IViewBase
    {
        void Close();
        void ShowAddressBook(AddressBookViewModel addressBookViewModel, DateOnly startDate);
        void OnParticipantsSet();
        void SetRecurrentMeetingActive(bool recurrentMeetingActive);
        void DisableWhileLoadingStateHolder();
        void EnableAfterLoadingStateHolder();
        void StartLoadingStateHolder();
        void OnModificationOccurred(IMeeting meeting, bool isDeleted);
    }
}