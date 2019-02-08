using System.Collections.Generic;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDataSources
	{
		public List<KeyValuePair<string, int>> Datasources = new List<KeyValuePair<string, int>>();

		public void Add(string sourceId, int datasourceId)
		{
			Datasources.Add(new KeyValuePair<string, int>(sourceId, datasourceId));
		}
	}
}