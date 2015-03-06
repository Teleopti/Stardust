using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class DictionaryToPostData : IDictionaryToPostData
	{
		public string Convert(IEnumerable<KeyValuePair<string, string>> data)
		{
			var items = from kvp in data
				select kvp.Key + "=" + kvp.Value;
			return string.Join("&", items);
		}
	}
}