using Newtonsoft.Json;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class NewtonsoftJsonSerializer : IJsonSerializer
	{
		public string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value);
		}
	}
}