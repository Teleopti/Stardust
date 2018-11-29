using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
{
	public class CurrentBelongsToDate 
	{
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public CurrentBelongsToDate(IPersonRepository personRepository, INow now)
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