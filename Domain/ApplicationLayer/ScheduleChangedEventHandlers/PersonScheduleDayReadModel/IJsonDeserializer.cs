namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IJsonDeserializer
	{
		T DeserializeObject<T>(string value);
	}
}