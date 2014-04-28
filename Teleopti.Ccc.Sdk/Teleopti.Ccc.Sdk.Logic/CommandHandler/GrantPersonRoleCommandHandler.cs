using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
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
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				var role = _applicationRoleRepository.Load(command.RoleId);
				if (person != null && role != null)
				{
					var permissionInformation = person.PermissionInformation;
					if (!permissionInformation.ApplicationRoleCollection.Contains(role))
					{
						permissionInformation.AddApplicationRole(role);
						uow.PersistAll();
					}
				}
			}
		}
	}
}