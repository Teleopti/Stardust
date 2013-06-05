using System.Dynamic;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Newtonsoft")]
	public class NewtonsoftJsonSerializer : IJsonSerializer
	{
		public string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Newtonsoft")]
	public class NewtonsoftJsonDeserializer : IJsonDeserializer
	{
		public dynamic DeserializeObject(string value)
		{
			return JsonConvert.DeserializeObject<ExpandoObject>(value);
		}
	}
}