namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IStateQueueWriter
	{
		void Enqueue(BatchInputModel model);
	}
}