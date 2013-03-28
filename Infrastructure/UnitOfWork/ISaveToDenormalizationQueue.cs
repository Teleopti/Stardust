using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface ISaveToDenormalizationQueue
	{
		void Execute<T>(T message) where T : IRaptorDomainMessageInfo;
	}
}