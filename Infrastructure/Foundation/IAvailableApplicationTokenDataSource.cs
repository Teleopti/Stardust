using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IAvailableApplicationTokenDataSource
	{
		bool IsDataSourceAvailable(IDataSource dataSource, string userIdentifier);
	}
}