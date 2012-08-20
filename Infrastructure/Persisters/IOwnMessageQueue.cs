namespace Teleopti.Ccc.Infrastructure.Persisters
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface IOwnMessageQueue
	{
		void ReassociateDataWithAllPeople();
		void NotifyMessageQueueSize();
	}
}