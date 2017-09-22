using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetAllRolesQueryHandler : IHandleQuery<GetAllRolesQueryDto, ICollection<RoleDto>>
	{
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public GetAllRolesQueryHandler(IApplicationRoleRepository applicationRoleRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_applicationRoleRepository = applicationRoleRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public ICollection<RoleDto> Handle(GetAllRolesQueryDto query)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (uow.LoadDeletedIfSpecified(query.LoadDeleted))
				{
					return _applicationRoleRepository.LoadAllApplicationRolesSortedByName().Select(s => new RoleDto
					{
						Id = s.Id,
						Name = s.Name,
						IsDeleted = ((IDeleteTag)s).IsDeleted
					}).ToList();
				}
			}
		}
	}
}