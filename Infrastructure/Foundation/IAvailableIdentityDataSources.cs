using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IAvailableIdentityDataSources
	{
		IEnumerable<IDataSource> AvailableDataSources(IEnumerable<IDataSource> dataSourcesToScan, string identity);
	}
}