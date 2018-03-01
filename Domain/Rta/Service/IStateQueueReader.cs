namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IStateQueueReader
	{
		BatchInputModel Dequeue();
	}
}