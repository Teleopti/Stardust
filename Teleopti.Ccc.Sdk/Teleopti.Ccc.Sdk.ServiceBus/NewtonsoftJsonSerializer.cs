using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class NewtonsoftJsonSerializer : IJsonSerializer
	{
		public string SerializeObject(object obj)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
		}
	}
}