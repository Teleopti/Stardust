using System.Collections.Concurrent;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IDataSourceReader
	{
		ConcurrentDictionary<string, int> Datasources();
	}
}