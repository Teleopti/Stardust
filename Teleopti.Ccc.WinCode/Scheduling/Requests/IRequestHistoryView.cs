using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public interface IRequestHistoryView
    {
        Guid SelectedPerson { get; }
        int StartRow { get; set; }
        void FillRequestList(ListViewItem[] listViewItems);
        IRequestHistoryLightWeight SelectedRequest { get; }
        int TotalCount { get; set; }
        int PageSize { get; }
        void ShowRequestDetails(string details);
        void ShowForm();
        void FillPersonCombo(ICollection<IRequestPerson> persons, Guid preSelectedPerson);
        void SetNextEnabledState(bool enabled);
        void SetPreviousEnabledState(bool enabled);
    }

    public interface IRequestPerson
    {
        string Name { get; set; }
        Guid Id { get; set; }
    }
}