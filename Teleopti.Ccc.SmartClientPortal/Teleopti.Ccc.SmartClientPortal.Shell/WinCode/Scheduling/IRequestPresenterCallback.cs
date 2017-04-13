namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IRequestPresenterCallback
	{
		void CommitUndo();
		void RollbackUndo();
	}
}