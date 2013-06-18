namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IJsonDeserializer<out T>
	{
		T DeserializeObject(string value);
	}
}