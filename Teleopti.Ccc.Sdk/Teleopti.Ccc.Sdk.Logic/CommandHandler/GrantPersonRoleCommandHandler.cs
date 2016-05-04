using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class GrantPersonRoleCommandHandler : IHandleCommand<GrantPersonRoleCommandDto>
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IApplicationRoleRepository _applicationRoleRepository;

		public GrantPersonRoleCommandHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IPersonRepository personRepository, IApplicationRoleRepository applicationRoleRepository)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personRepository = personRepository;
			_applicationRoleRepository = applicationRoleRepository;
		}

		public void Handle(GrantPersonRoleCommandDto command)
		{
            var result = new CommandResultDto { AffectedItems = 0, AffectedId = command.PersonId };
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Get(command.PersonId);
				checkIfAuthorized(person);

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

		private static void checkIfAuthorized(IPerson person)
		{
			if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage, DateOnly.Today, person))
			{
				throw new FaultException("You're not allowed to modify roles for this person.");
			}
		}
	}
}