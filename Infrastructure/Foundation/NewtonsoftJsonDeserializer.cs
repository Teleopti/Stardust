using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Newtonsoft")]
	public class NewtonsoftJsonDeserializer<T> : IJsonDeserializer<T>
	{
		public T DeserializeObject(string value)
		{
			return JsonConvert.DeserializeObject<T>(value);
		}
	}
}