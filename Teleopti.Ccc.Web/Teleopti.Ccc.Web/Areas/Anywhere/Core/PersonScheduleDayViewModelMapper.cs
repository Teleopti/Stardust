using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleDayViewModelMapper : IPersonScheduleDayViewModelMapper
	{
		public PersonScheduleDayViewModel Map(PersonScheduleDayReadModel data)
		{
			return Mapper.Map<PersonScheduleDayReadModel, PersonScheduleDayViewModel>(data);
		}
	}
}