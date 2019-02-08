using System;
using System.Collections.Generic;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		}

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