namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateQueueWriter
	{
		void Enqueue(BatchInputModel model);
	}
}