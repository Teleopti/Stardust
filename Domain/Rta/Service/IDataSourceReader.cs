using System.Collections.Concurrent;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IDataSourceReader
	{
		ConcurrentDictionary<string, int> Datasources();
	}
}