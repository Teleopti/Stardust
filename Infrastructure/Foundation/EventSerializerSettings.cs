using System;
using System.Collections.Generic;
using Hangfire.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
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
			ContractResolver = new ShortCommonPropertyMappingContractResolver();
			SerializationBinder = new MappingSerializationBinder(_typeMapper);
			Converters = new List<JsonConverter> {new InvocationDataConverter(_typeMapper)};
		}

		public Type TypeResolver(string typeName)
		{
			if (_typeMapper.TypeForPersistedName(typeName, out var type))
				return type;
			typeName = typeName.Replace("System.Private.CoreLib", "mscorlib");
			return Type.GetType(typeName, true, true);
		}

		// for hangfire
		// InvocationData.Type and InvocationData.ParameterTypes
		// to be persisted with short names
		private class InvocationDataConverter : JsonConverter
		{
			private readonly string _hangfireEventServerPersistedTypeName;
			private const string HangfireEventServerMethodName = "Process";
			private const string HangfireEventServerParameterTypes = "[\"System.String, mscorlib\",\"HangfireEventJob\"]";

			public InvocationDataConverter(PersistedTypeMapper typeMapper)
			{
				_hangfireEventServerPersistedTypeName = typeMapper.NameForPersistence(typeof(HangfireEventServer));
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var invocationData = value as InvocationData;
				writer.WriteStartObject();
				writer.WritePropertyName("Type");
				writer.WriteValue(_hangfireEventServerPersistedTypeName);
				writer.WritePropertyName("Method");
				writer.WriteValue(HangfireEventServerMethodName);
				writer.WritePropertyName("ParameterTypes");
				writer.WriteValue(HangfireEventServerParameterTypes);
				writer.WritePropertyName("Arguments");
				writer.WriteValue(invocationData.Arguments);
				writer.WriteEndObject();
			}

			public override bool CanRead => false;

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
				throw new NotImplementedException();

			public override bool CanConvert(Type objectType) =>
				typeof(InvocationData) == objectType;
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
				if (_typeMapper.NameForPersistence(serializedType, out typeName))
					assemblyName = null;
				else
					base.BindToName(serializedType, out assemblyName, out typeName);
			}

			public override Type BindToType(string assemblyName, string typeName)
			{
				if (_typeMapper.TypeForPersistedName(typeName, assemblyName, out var bindToType))
					return bindToType;
				return base.BindToType(assemblyName, typeName);
			}
		}

		private class ShortCommonPropertyMappingContractResolver : DefaultContractResolver
		{
			private readonly Dictionary<string, string> propertyMappings;

			public ShortCommonPropertyMappingContractResolver()
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