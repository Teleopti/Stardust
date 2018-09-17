using System.Collections.Concurrent;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public interface IDataSourceReader
	{
		ConcurrentDictionary<string, int> Datasources();
	}
}