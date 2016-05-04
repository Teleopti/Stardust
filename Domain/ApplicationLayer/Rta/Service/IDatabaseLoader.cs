using System.Collections.Concurrent;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IDatabaseLoader
	{
		ConcurrentDictionary<string, int> Datasources();
	}
}