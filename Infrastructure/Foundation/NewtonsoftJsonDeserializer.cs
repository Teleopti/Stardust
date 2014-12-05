using System;
using Newtonsoft.Json;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class NewtonsoftJsonDeserializer : IJsonDeserializer
	{
		public T DeserializeObject<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value);
		}

		public object DeserializeObject(string value, Type type)
		{
			return JsonConvert.DeserializeObject(value, type);
		}
	}
}