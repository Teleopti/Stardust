namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IJsonSerializer
	{
		string SerializeObject(object obj);
	}
}