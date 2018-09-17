namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public interface IStateQueueReader
	{
		BatchInputModel Dequeue();
	}
}