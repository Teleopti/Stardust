namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public interface IStateQueueWriter
	{
		void Enqueue(BatchInputModel model);
	}
}