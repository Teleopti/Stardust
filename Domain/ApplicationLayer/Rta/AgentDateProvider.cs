using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAgentDateProvider
	{
		DateOnly Get(Guid personId);
	}
	public class AgentDateProvider : IAgentDateProvider
	{
		private readonly INow _now;
		private readonly IPersonRepository _personRepository;

		public AgentDateProvider(INow now, IPersonRepository personRepository)
		{
			_now = now;
			_personRepository = personRepository;
		}

		public DateOnly Get(Guid personId)
		{
			return
				new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(),
					_personRepository.Get(personId).PermissionInformation.DefaultTimeZone()));
		}
	}
}