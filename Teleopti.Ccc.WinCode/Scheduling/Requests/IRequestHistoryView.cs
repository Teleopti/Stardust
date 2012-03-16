using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public interface IRequestHistoryView
    {
        Guid SelectedPerson { get; }
        int StartRow { get; }
        void FillRequestList(ListViewItem[] listViewItems);
        IRequestHistoryLightWeight SelectedRequest { get; }
        void ShowRequestDetails(string details);
        void ShowForm();
        void FillPersonCombo(ICollection<IRequestPerson> persons, Guid preSelectedPerson);
    }

    public interface IRequestPerson
    {
        string Name { get; set; }
        Guid Id { get; set; }
    }
}