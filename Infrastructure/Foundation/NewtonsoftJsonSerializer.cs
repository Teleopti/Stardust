using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class NewtonsoftJsonSerializer :
		IJsonSerializer,
		IJsonDeserializer,
		IJsonEventSerializer,
		IJsonEventDeserializer
	{
		private readonly EventSerializerSettings _settings;

		public NewtonsoftJsonSerializer(EventSerializerSettings settings)
		{
			_settings = settings;
		}

		public string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value);
		}

		public T DeserializeObject<T>(string value)
		{
			return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
		}

		public object DeserializeObject(string value, Type type)
		{
			return JsonConvert.DeserializeObject(value, type);
		}

		public string SerializeEvent(object value)
		{
			return JsonConvert.SerializeObject(value, _settings);
		}

		public object DeserializeEvent(string value, Type type)
		{
			return JsonConvert.DeserializeObject(value, type, _settings);
		}

		public static NewtonsoftJsonSerializer Make()
		{
			return new NewtonsoftJsonSerializer(null);
		}
	}
}