using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class NewtonsoftJsonDeserializer : IJsonDeserializer
	{
		public T DeserializeObject<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value);
		}
	}
}