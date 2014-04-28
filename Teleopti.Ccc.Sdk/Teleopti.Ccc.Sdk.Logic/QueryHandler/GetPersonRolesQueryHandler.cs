using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonRolesQueryHandler : IHandleQuery<GetPersonRolesQueryDto, ICollection<RoleDto>>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public GetPersonRolesQueryHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public ICollection<RoleDto> Handle(GetPersonRolesQueryDto query)
		{
			using (_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(query.PersonId);
				return
					person.PermissionInformation.ApplicationRoleCollection.Select(s => new RoleDto
					{
						Id = s.Id,
						Name = s.Name
					}).ToList();
			}
		}
	}
}