using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Interfaces
{
    public interface IAddressBookView
    {
        void SetCurrentDate(DateOnly startDate);
        void SetRequiredParticipants(string requiredParticipants);
        void SetOptionalParticipants(string optionalParticipants);
        void PerformSearch();
        void PrepareGridView(IList<ContactPersonViewModel> personViewDataList);
        string GetSearchCriteria();
    }
}