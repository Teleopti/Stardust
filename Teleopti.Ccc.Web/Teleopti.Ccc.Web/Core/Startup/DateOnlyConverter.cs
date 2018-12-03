using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Core.Startup
{
	public class DateOnlyConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, ((DateOnly)value).Date);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return new DateOnly(serializer.Deserialize<DateTime>(reader));
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof (DateOnly) == objectType;
		}
	}
}