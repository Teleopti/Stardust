namespace Teleopti.Wfm.Adherence.States
{
	public interface IStateQueueWriter
	{
		void Enqueue(BatchInputModel model);
	}
}