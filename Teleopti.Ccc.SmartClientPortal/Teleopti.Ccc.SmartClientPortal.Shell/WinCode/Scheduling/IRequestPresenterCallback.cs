namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IRequestPresenterCallback
	{
		void CommitUndo();
		void RollbackUndo();
	}
}