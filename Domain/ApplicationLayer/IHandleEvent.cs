namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IHandleEvent<TEvent>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event")]
		void Handle(TEvent @event);
	}
}