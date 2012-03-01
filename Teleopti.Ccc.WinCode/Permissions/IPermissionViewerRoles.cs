using System;
using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Permissions
{
    public interface IPermissionViewerRoles
    {
        ListView RolesMainList { get;}
        ListView PersonsMainList { get; }
        Guid SelectedPerson { get; }
        ListView PersonRolesList { get; }
        ListView PersonFunctionsList { get; }
        ListView FunctionsMainList { get; }
        Guid SelectedFunction { get; }
        ListView FunctionPersonsList { get; }
        ListView FunctionRolesList { get; }
        void Show();
    }
}