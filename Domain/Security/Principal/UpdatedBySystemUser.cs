using System;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class UpdatedBySystemUser : IUpdatedBySystemUser
	{
		private readonly IUpdatedByScope _updatedByScope;
		private readonly IPersonRepository _personRepository;

		public UpdatedBySystemUser(IUpdatedByScope updatedByScope, IPersonRepository personRepository)
		{
			_updatedByScope = updatedByScope;
			_personRepository = personRepository;
		}

		public IDisposable Context()
		{
			var systemUser = _personRepository.Get(SystemUser.Id);
			_updatedByScope.OnThisThreadUse(systemUser);
			return new GenericDisposable(() => _updatedByScope.OnThisThreadUse(null));
		}
	}
}