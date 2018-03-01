namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IStateQueueReader
	{
		BatchInputModel Dequeue();
	}
}