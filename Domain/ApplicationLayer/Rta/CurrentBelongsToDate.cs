using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ICurrentBelongsToDate
	{
		DateOnly ForPerson(Guid personId);
	}

	public class CurrentBelongsToDateFromUtcNow : ICurrentBelongsToDate
	{
		private readonly INow _now;

		public CurrentBelongsToDateFromUtcNow(INow now)
		{
			_now = now;
		}

		public DateOnly ForPerson(Guid personId)
		{
			return new DateOnly(_now.UtcDateTime());
		}
	}

	public class CurrentBelongsToDateFromPersonsCurrentTime : ICurrentBelongsToDate
	{
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public CurrentBelongsToDateFromPersonsCurrentTime(IPersonRepository personRepository, INow now)
		{
			_personRepository = personRepository;
			_now = now;
		}

		public DateOnly ForPerson(Guid personId)
		{
			var person = _personRepository.Get(personId);
			return person != null ?
				new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), person.PermissionInformation.DefaultTimeZone())) :
				new DateOnly(_now.UtcDateTime());
		}
	}
}