using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
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
