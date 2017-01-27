using System.Collections;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeKeyValueStorePersister : IKeyValueStorePersister
	{
		private Hashtable _data = new Hashtable();

		public void Update(string key, string value)
		{
			_data[key] = value;
		}

		public string Get(string key)
		{
			return _data[key] as string;
		}

		public void Delete(string key)
		{
			_data[key] = null;
		}
	}
}