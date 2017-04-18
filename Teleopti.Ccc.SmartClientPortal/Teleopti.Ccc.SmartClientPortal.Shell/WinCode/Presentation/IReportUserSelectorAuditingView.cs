using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
    public interface IReportUserSelectorAuditingView
    {
        void UpdateUsersCombo(ReadOnlyCollection<ReportUserSelectorAuditingModel> userList);
        void SetSelectedUser(ReportUserSelectorAuditingModel reportUserSelectorAuditingModel);
        void Initialize();
        IList<IPerson> SelectedUsers { get; }
        ReportUserSelectorAuditingModel SelectedUserModel { get; }
    }
}
