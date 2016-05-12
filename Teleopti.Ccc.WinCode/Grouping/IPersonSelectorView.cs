using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Interfaces.Domain;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;

namespace Teleopti.Ccc.WinCode.Grouping
{
    public interface IPersonSelectorView: IDisposable
	{
		event EventHandler DoFilter;

        void ResetTabs(TabPageAdv[] tabs, IExecutableCommand mainTabLoadCommand);
        DateOnly SelectedDate { get; }
        void ResetTreeView(object treeNodeAdv);
        IList<TreeNodeAdv> SelectedNodes { get; }
        IList<TreeNodeAdv> AllNodes { get; }
        TabPageAdv SelectedTab { get; }
        TabControlAdv TabControl { get; }
        void AddNewGroupPage();
        void DeleteGroupPage(Guid id, string name);
        void ModifyGroupPage(Guid id);
        void AddNewPageTab(TabPageAdv tabPageAdv);
        void RenameGroupPage(Guid id, string oldName);
        void ShowDataSourceException(DataSourceException dataSourceException, string dialogTitle);
        bool OpenEnabled { get; set; }
        bool FindVisible { get; set; }
        bool ShowCheckBoxes { get; set; }
        bool ShowDateSelection { get; set; }
        Cursor Cursor { get; set; }
        IEnumerable<Guid> PreselectedPersonIds { get; set; }
		IEnumerable<Guid> VisiblePersonIds { get; set; }
        bool HideMenu { get; set; }
        DateOnlyPeriod SelectedPeriod { get; set; }
		bool KeepInteractiveOnDuringLoad { get; set; }
		bool ExpandSelected { get; set; }
    }
}