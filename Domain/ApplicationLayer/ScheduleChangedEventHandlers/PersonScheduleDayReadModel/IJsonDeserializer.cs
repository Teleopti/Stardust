namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IJsonDeserializer
	{
		dynamic DeserializeObject(string value);
	}
}