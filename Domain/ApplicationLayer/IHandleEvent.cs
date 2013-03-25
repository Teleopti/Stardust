namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IHandleEvent<TEvent>
	{
		void Handle(TEvent @event);
	}
}