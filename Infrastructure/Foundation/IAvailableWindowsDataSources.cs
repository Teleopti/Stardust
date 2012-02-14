using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IAvailableWindowsDataSources
	{
		IEnumerable<IDataSource> AvailableDataSources(IEnumerable<IDataSource> dataSourcesToScan,
		                                                              string domainName,
		                                                              string userName);
	}
}