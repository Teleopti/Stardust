using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public static class PersistedTypeMapperExtensions
	{
		public static bool tryMapPersistedName(this PersistedTypeMapper typeMapper, string assemblyName, string typeName, out Type bindToType)
		{
			if (assemblyName == null)
			{
				bindToType = typeMapper.TypeForPersistedName(typeName);
				return true;
			}

			if (mapPersistedName(assemblyName))
			{
				bindToType = typeMapper.TypeForPersistedName($"{typeName}, {assemblyName}");
				return true;
			}

			bindToType = null;
			return false;
		}
		
		public static bool mapPersistedName(string persistedName)
		{
			Console.WriteLine($"mapPersistedTypeName {persistedName}");
			if (persistedName.StartsWith("Teleopti."))
				return true;
			if (persistedName.IndexOfAny(new[] {',', '.'}) < 0)
				return true;
			return false;
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
			ContractResolver = new ShortCommonPropertyMappingContractResolver();
			SerializationBinder = new MappingSerializationBinder(_typeMapper);
			//Converters = new List<JsonConverter> {new TypeArrayConverter(_typeMapper)};
			Converters = new List<JsonConverter>
			{
				new TypeArrayConverter(_typeMapper),
				//new TypeConverter(_typeMapper)
			};
		}

		public Type TypeResolver(string typeName)
		{
			if (PersistedTypeMapperExtensions.mapPersistedName(typeName))
			{
				var info = typeName.Split(',');
				if (info.Length == 1)
					return _typeMapper.TypeForPersistedName($"{info[0]}");
				var name = info[0];
				var assemblyName = info[1];
				return _typeMapper.TypeForPersistedName($"{name},{assemblyName}");
			}

			typeName = typeName.Replace("System.Private.CoreLib", "mscorlib");
			return Type.GetType(typeName, true, true);
		}

		private static bool mapType(Type serializedType)
		{
			Console.WriteLine($"mapType {serializedType.Assembly.FullName}");
			return serializedType.Assembly.FullName.StartsWith("Teleopti.");
		}

		
		
//
//		private class TypeConverter : JsonConverter
//		{
//			private readonly PersistedTypeMapper _typeMapper;
//
//			public TypeConverter(PersistedTypeMapper typeMapper)
//			{
//				_typeMapper = typeMapper;
//			}
//
//			public override bool CanRead => false;
//
//			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//			{
//				Console.WriteLine($"WriteJson/Type/{value.ToString()}");
//				writer.WriteValue(value.ToString());
//				Console.WriteLine($"/WriteJson/Type");
//				Console.WriteLine();
//			}
//
//			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//			{
//				throw new NotImplementedException();
////				Console.WriteLine("ReadJson/Type");
////				Type type = null;
////				do
////				{
////					if (reader.Value == null)
////						continue;
////
////					Console.WriteLine($"ReadJson/Type/{reader.Value}");
////					var typeName = reader.Value as string;
////					if (typeName.Contains("HangfireEventJob"))
////					{
////						var info = typeName.Split(',');
////						var name = info[0];
////						var assemblyName = info[1];
////						var typeName2 = $"{name},{assemblyName}";
////						type = _typeMapper.TypeForPersistedName(typeName2);
////					}
////					else
////					{
////						type = Type.GetType(reader.Value as string);
////					}
////
////					break;
////				} while (reader.Read());
////
////				Console.WriteLine($"/ReadJson/Type/{type}");
////				Console.WriteLine();
////				return type;
//			}
//
//			public override bool CanConvert(Type objectType)
//			{
//				return typeof(Type) == objectType;
//			}
//		}
//

		private class TypeArrayConverter : JsonConverter
		{
			private readonly PersistedTypeMapper _typeMapper;

			public TypeArrayConverter(PersistedTypeMapper typeMapper)
			{
				_typeMapper = typeMapper;
			}

			public override bool CanRead => false;

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				Console.WriteLine($"WriteJson/TypeArray/{value.ToString()}");

				var types = (Type[]) value;
				writer.WriteStartArray();
				types.ForEach(t =>
				{
					if (mapType(t))
						serializer.Serialize(writer, _typeMapper.NameForPersistence(t));
					else
						serializer.Serialize(writer, t);
				});
				writer.WriteEndArray();

				Console.WriteLine($"/WriteJson/TypeArray");
				Console.WriteLine();
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				throw new NotImplementedException();
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
			}

			public override bool CanConvert(Type objectType)
			{
				return typeof(Type[]) == objectType;
			}
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
				if (mapType(serializedType))
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
				if (_typeMapper.tryMapPersistedName(assemblyName, typeName, out var bindToType)) 
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