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
	public class RevokePersonRoleCommandHandler : IHandleCommand<RevokePersonRoleCommandDto>
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IApplicationRoleRepository _applicationRoleRepository;

		public RevokePersonRoleCommandHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IPersonRepository personRepository, IApplicationRoleRepository applicationRoleRepository)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personRepository = personRepository;
			_applicationRoleRepository = applicationRoleRepository;
		}

		public void Handle(RevokePersonRoleCommandDto command)
		{
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				checkIfAuthorized(person);

				var role = _applicationRoleRepository.Load(command.RoleId);
				if (person != null && role != null)
				{
					var permissionInformation = person.PermissionInformation;
					if (permissionInformation.ApplicationRoleCollection.Contains(role))
					{
						permissionInformation.RemoveApplicationRole(role);
						uow.PersistAll();
					}
				}
			}
		}

		private static void checkIfAuthorized(IPerson person)
		{
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage, DateOnly.Today, person))
			{
				throw new FaultException("You're not allowed to modify roles for this person.");
			}
		}
	}
}