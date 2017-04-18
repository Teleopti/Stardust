namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IPresenterBase :  IValidate
    {
        IDataHelper DataWorkHelper { get; }

        IExplorerPresenter Explorer { get; }

		void LoadModelCollection();
    }
}
