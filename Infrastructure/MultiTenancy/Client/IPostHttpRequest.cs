using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IPostHttpRequest
	{
		T Send<T>(string url, string userAgent, IEnumerable<KeyValuePair<string, string>> data);
	}
}