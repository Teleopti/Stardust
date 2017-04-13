using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Shifts.Views;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IExplorerPresenter
    {
        IExplorerView View { get; }

        IExplorerViewModel Model { get; }

        IDataHelper DataWorkHelper { get; }

        INavigationPresenter NavigationPresenter { get; }

        IGeneralPresenter GeneralPresenter { get; }

        IVisualizePresenter VisualizePresenter { get; }

        bool Validate();

        bool Persist();

        bool CheckForUnsavedData();

		void Show(Form mainWindow);

    }
}
