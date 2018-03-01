namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IRtaEventPublisher
	{
		void Publish(Context info);
	}
}