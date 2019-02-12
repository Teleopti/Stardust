using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teleopti.Ccc.Domain.Collection;
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


	public class EventSerializerSettings : JsonSerializerSettings
	{
		private readonly PersistedTypeMapper _typeMapper;

		public EventSerializerSettings(PersistedTypeMapper typeMapper)
		{
			_typeMapper = typeMapper;
			//Maybe this is a good idea? ;)
			//DateTimeZoneHandling = DateTimeZoneHandling.Utc,
			TypeNameHandling = TypeNameHandling.Auto;
			NullValueHandling = NullValueHandling.Ignore;
			ContractResolver = new customContractResolver();
			SerializationBinder = new MappingSerializationBinder(_typeMapper);
			//Converters = new List<JsonConverter> {new TypeArrayConverter(_typeMapper)};
		}

		public Type TypeResolver(string typeName)
		{
			if (typeName.Contains("HangfireEventJob"))
			{
				var info = typeName.Split(',');
				var name = info[0];
				var assemblyName = info[1];
				return _typeMapper.TypeForPersistedName($"{name},{assemblyName}");
			}

			typeName = typeName.Replace("System.Private.CoreLib", "mscorlib");
			return System.Type.GetType(typeName, true, true);
		}

//
//		private class TypeArrayConverter : JsonConverter
//		{
//			private readonly PersistedTypeMapper _typeMapper;
//
//			public TypeArrayConverter(PersistedTypeMapper typeMapper)
//			{
//				_typeMapper = typeMapper;
//			}
//			
//			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//			{
//				var types = (Type[]) value;
//				writer.WriteStartArray();
//				types.ForEach(t =>
//				{
//					serializer.Serialize(writer, t);
//				});
//				writer.WriteEndArray();
//			}
//
//			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//			{
//				var types = Enumerable.Empty<Type>();
//				while (reader.Read() && reader.Value != null)
//				{
//					var typeName = reader.Value as string;
//					if (typeName.Contains("HangfireEventJob"))
//					{
//						var info = typeName.Split(',');
//						var name = info[0];
//						var assemblyName = info[1];
//						var typeName2 = $"{name},{assemblyName}"; 
//						types = types.Append(_typeMapper.TypeForPersistedName(typeName2));
//					}
////					else if (typeName.Contains("Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Ccc.Infrastructure"))
////						types = types.Append(typeof(HangfireEventJob));
//					else
//						types = types.Append(serializer.Deserialize<Type>(reader));
//				}
//
//				return types.ToArray();
//			}
//
//			public override bool CanConvert(Type objectType)
//			{
//				return typeof(Type[]) == objectType;
//			}
//		}

		private class MappingSerializationBinder : DefaultSerializationBinder
		{
			private readonly PersistedTypeMapper _typeMapper;

			public MappingSerializationBinder(PersistedTypeMapper typeMapper)
			{
				_typeMapper = typeMapper;
			}

			public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
			{
				if (serializedType.Assembly.FullName.StartsWith("Teleopti."))
				{
					typeName = _typeMapper.NameForPersistence(serializedType);
					assemblyName = null;
				}
				else
				{
					base.BindToName(serializedType, out assemblyName, out typeName);
				}
			}

			public override Type BindToType(string assemblyName, string typeName)
			{
				if (assemblyName == null)
					return _typeMapper.TypeForPersistedName(typeName);
				if (assemblyName.StartsWith("Teleopti."))
					return _typeMapper.TypeForPersistedName($"{typeName}, {assemblyName}");
				return base.BindToType(assemblyName, typeName);
			}
		}

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
					{"Adherence", "a"},
					{"AdherenceWithPreviousActivity", "ap"},
					{"InOrNeutralAdherenceWithPreviousActivity", "ax"},
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