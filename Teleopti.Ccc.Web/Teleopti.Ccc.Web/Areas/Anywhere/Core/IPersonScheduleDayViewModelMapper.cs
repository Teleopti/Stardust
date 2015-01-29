using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IPersonScheduleDayViewModelMapper
	{
		PersonScheduleDayViewModel Map(PersonScheduleDayReadModel data);
	}
}