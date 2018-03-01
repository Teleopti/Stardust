using System.Collections.Concurrent;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IDataSourceReader
	{
		ConcurrentDictionary<string, int> Datasources();
	}
}