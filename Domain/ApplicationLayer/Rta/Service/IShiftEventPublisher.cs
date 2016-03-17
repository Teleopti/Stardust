namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IShiftEventPublisher
	{
		void Publish(Context info);
	}
}