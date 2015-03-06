using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IDictionaryToPostData
	{
		string Convert(IEnumerable<KeyValuePair<string, string>> data);
	}
}