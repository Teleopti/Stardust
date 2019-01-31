namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ICurrentEventPublisher
	{
		IEventPublisher Current();
	}
}