namespace Teleopti.Wfm.Adherence.States
{
	public interface IStateQueueReader
	{
		BatchInputModel Dequeue();
		int Count();
	}
}