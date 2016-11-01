using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
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
		    var result = new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 0};
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Get(command.PersonId);
				person.VerifyCanBeModifiedByCurrentUser();

				var role = _applicationRoleRepository.Get(command.RoleId);
				if (person != null && role != null)
				{
					var permissionInformation = person.PermissionInformation;
					if (permissionInformation.ApplicationRoleCollection.Contains(role))
					{
						permissionInformation.RemoveApplicationRole(role);
						uow.PersistAll();

					    result.AffectedItems = 1;
					}
				}
			}
		    command.Result = result;
		}
	}
}