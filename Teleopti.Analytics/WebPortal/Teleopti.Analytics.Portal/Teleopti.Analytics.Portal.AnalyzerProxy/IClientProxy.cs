using System;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;

namespace Teleopti.Analytics.Portal.AnalyzerProxy
{
    public interface IClientProxy
    {
        CatalogItem[] ReportCollection();
        ReportInstance OpenReport(int reportId, PermissionLevel userPermissions);
        void CloseReport(ReportInstance reportInstance, bool saveBeforeClose);
        Role[] GetRoles();
        User[] GetUsers();
        Role[] GetUserRoles(string userName);
        bool DeleteUserRoleMembership(int userId, int roleId);
        bool DeleteUser(User user);
        bool DeleteReport(int reportId);
        User AddUser(string id);
        bool EnsureDataSourceExists();
        CatalogItem AnalyzerDataSource { get; }
        CatalogItem SharedReportsFolder { get; }
        int NewReport(CatalogItem report);
        PermissionLevel GetUserPermissions(string userName);
        bool AssignRoleMembership(int userId, int roleId);
        bool SetDefaultRolePermission(int generalUserRoleId, int reportDesignerRoleId);
        Uri GetReportUrl(ReportInstance reportInstance);
        bool DoReportExist(string name);
    }

    public enum PermissionLevel
    {
        // Below is a list of Analyzers permission
        // 1 – List only
        // 4 – Config only
        // 5 – List and Config
        // 16 – Execute only
        // 17 – List and Execute
        // 20 – Execute and Config
        // 64 – Export only
        // 65 – List and Export
        // 68 – Export and config
        // 80 – Execute and Export
        // 81 – List, Execute and Export
        // 84 – Execute, Export and Config
        // 85 – List, Execute, Export and Config
        None = 0,
        GeneralUser = 81,
        ReportDesigner = 85
    } ;

}