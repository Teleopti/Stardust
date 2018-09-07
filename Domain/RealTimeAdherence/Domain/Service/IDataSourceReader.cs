using System.Collections.Concurrent;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IDataSourceReader
	{
		ConcurrentDictionary<string, int> Datasources();
	}
}