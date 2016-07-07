namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IPresenterBase :  IValidate
    {
        IDataHelper DataWorkHelper { get; }

        IExplorerPresenter Explorer { get; }

		void LoadModelCollection();
    }
}
