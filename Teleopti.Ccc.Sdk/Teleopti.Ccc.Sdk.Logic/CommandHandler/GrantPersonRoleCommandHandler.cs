using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class GrantPersonRoleCommandHandler : IHandleCommand<GrantPersonRoleCommandDto>
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		private readonly ICurrentAuthorization _currentAuthorization;

		public GrantPersonRoleCommandHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IPersonRepository personRepository, IApplicationRoleRepository applicationRoleRepository, ICurrentAuthorization currentAuthorization)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personRepository = personRepository;
			_applicationRoleRepository = applicationRoleRepository;
			_currentAuthorization = currentAuthorization;
		}

		public void Handle(GrantPersonRoleCommandDto command)
		{
            var result = new CommandResultDto { AffectedItems = 0, AffectedId = command.PersonId };
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Get(command.PersonId);
				person.VerifyCanBeModifiedByCurrentUser(_currentAuthorization);

				var role = _applicationRoleRepository.Get(command.RoleId);
				if (person != null && role != null)
				{
					var permissionInformation = person.PermissionInformation;
					if (!permissionInformation.ApplicationRoleCollection.Contains(role))
					{
						permissionInformation.AddApplicationRole(role);
						uow.PersistAll();

					    result.AffectedItems = 1;
					}
				}
			}
		    command.Result = result;
		}
	}
}