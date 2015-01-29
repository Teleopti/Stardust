using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleDayViewModelFactory : IPersonScheduleDayViewModelFactory
	{
		private readonly IPersonScheduleDayViewModelMapper _personScheduleDayViewModelMapper;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;

		public PersonScheduleDayViewModelFactory(IPersonScheduleDayViewModelMapper personScheduleDayViewModelMapper, IPersonScheduleDayReadModelFinder personScheduleDayReadModelFinder)
		{
			_personScheduleDayViewModelMapper = personScheduleDayViewModelMapper;
			_personScheduleDayReadModelFinder = personScheduleDayReadModelFinder;
		}

		public PersonScheduleDayViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var data = _personScheduleDayReadModelFinder.ForPerson(new DateOnly(date.Year, date.Month, date.Day), personId);
			return _personScheduleDayViewModelMapper.Map(data);
		}
	}
}