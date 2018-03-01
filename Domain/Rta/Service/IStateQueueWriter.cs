namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IStateQueueWriter
	{
		void Enqueue(BatchInputModel model);
	}
}