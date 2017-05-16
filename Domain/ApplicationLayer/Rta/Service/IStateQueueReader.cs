namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateQueueReader
	{
		BatchInputModel Dequeue();
	}
}