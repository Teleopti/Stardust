using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface INavigationPanel
    {
        void Open();
        bool OpenEnabled { get; set; }
        bool AddGroupPageEnabled { get; set; }
        bool RenameGroupPageEnabled { get; set; }
        bool DeleteGroupPageEnabled { get; set; }
        bool ModifyGroupPageEnabled { get; set; }
        void ShowDataSourceException(DataSourceException dataSourceException, string dialogTitle);
        Cursor Cursor { get; set; }
    }
}