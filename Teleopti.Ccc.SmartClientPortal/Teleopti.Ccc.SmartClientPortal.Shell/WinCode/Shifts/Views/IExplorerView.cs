using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views
{
    public interface IExplorerView : IViewBase
    {
        void SetupHelpContextForGrid(GridControlBase grid);

        bool AskForDelete();

        bool CheckForSelectedRuleSet();

        void SetVisualGridDrawingInfo();

        void SelectToolStripItem(ShiftCreatorViewType view);

        void RefreshChildViews();

        void RefreshActivityGridView();

	    void RefreshVisualizeView();

        void AddControlHelpContext(Control control);

        IExplorerPresenter Presenter { get; set; }

        void ShowDataSourceException(DataSourceException dataSourceException);

        void Show(IExplorerPresenter explorerPresenter, Form mainWindow);

        void ForceRefreshNavigationView();

        void UpdateNavigationViewTreeIcons();

	    void SetViewEnabled(bool enabled);

	    void Activate();
        void ExitEditMode();
    }
}
