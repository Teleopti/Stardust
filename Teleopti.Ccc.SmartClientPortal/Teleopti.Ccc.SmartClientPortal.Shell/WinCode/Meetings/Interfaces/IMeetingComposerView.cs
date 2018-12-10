using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
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