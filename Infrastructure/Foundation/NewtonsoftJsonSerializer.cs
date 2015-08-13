using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class NewtonsoftJsonSerializer : 
		IJsonSerializer,
		IJsonDeserializer,
		IJsonEventSerializer,
		IJsonEventDeserializer
	{

		public string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value);
		}

		public T DeserializeObject<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value);
		}

		public object DeserializeObject(string value, Type type)
		{
			return JsonConvert.DeserializeObject(value, type);
		}



		public string SerializeEvent(object value)
		{
			return JsonConvert.SerializeObject(value, _eventSerializationSettings);
		}

		public object DeserializeEvent(string value, Type type)
		{
			return JsonConvert.DeserializeObject(value, type, _eventSerializationSettings);
		}

		private readonly JsonSerializerSettings _eventSerializationSettings = new JsonSerializerSettings
		{
			ContractResolver = new customContractResolver()
		};

		private class customContractResolver : DefaultContractResolver
		{
			private readonly Dictionary<string, string> propertyMappings;

			public customContractResolver()
			{
				propertyMappings = new Dictionary<string, string>
				{
					{"PersonId", "p"},
					{"BelongsToDate", "d"},
					{"Timestamp", "t"},
					{"Datasource", "ds"},
					{"BusinessUnitId", "bu"},
					{"InAdherence", "in"},
					{"InOrNeutralAdherenceWithPreviousActivity", "ax"},
					{"Adherence", "a"},
				};
			}

			protected override string ResolvePropertyName(string propertyName)
			{
				string resolvedName;
				var resolved = propertyMappings.TryGetValue(propertyName, out resolvedName);
				return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
			}
		}

	}
}