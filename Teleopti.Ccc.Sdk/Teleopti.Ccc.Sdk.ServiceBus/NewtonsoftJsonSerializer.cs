using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Newtonsoft")]
	public class NewtonsoftJsonSerializer : IJsonSerializer
	{
		public string SerializeObject(object value)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(value);
		}
	}
}