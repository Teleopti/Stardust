using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleDayViewModelFactory : IPersonScheduleDayViewModelFactory
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;

		public PersonScheduleDayViewModelFactory(IUserTimeZone userTimeZone, IPersonScheduleDayReadModelFinder personScheduleDayReadModelFinder)
		{
			_userTimeZone = userTimeZone;
			_personScheduleDayReadModelFinder = personScheduleDayReadModelFinder;
		}

		public PersonScheduleDayViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var data = _personScheduleDayReadModelFinder.ForPerson(new DateOnly(date), personId);

			var sourceTimeZone = _userTimeZone.TimeZone();
			return new PersonScheduleDayViewModel
			{
				Date = data.Date,
				Person = data.PersonId,
				StartTime =
					data.Start.HasValue ? TimeZoneHelper.ConvertFromUtc(data.Start.Value, sourceTimeZone) : (DateTime?) null,
				EndTime =
					data.End.HasValue ? TimeZoneHelper.ConvertFromUtc(data.End.Value, sourceTimeZone) : (DateTime?) null,
				IsDayOff = data.IsDayOff
			};
		}
	}
}