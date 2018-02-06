using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Models;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public interface IRoleManager
	{
		void GrantRoles(GrantRolesInputModel userRoles);
		void RevokeRoles(RevokeRolesInputModel userRoles);
	}

	public class RoleManager : IRoleManager
	{
		private readonly IPersonRepository _personRepository;
		private readonly IApplicationRoleRepository _roleRepository;

		public RoleManager(IPersonRepository personRepository, IApplicationRoleRepository roleRepository)
		{
			_roleRepository = roleRepository;
			_personRepository = personRepository;
		}

		public void GrantRoles(GrantRolesInputModel userRoles)
		{
			// Validate Input here
			var persons = _personRepository.FindPeople(userRoles.Persons);
			var roles = _roleRepository.LoadAll();
			var selectedRoles = roles.Where(x => userRoles.Roles.ToList().Contains(x.Id ?? Guid.Empty));

			foreach (var person in persons)
			{
				selectedRoles.ForEach(role => person.PermissionInformation.AddApplicationRole(role));
			}
		}

		public void RevokeRoles(RevokeRolesInputModel userRoles)
		{
			// Validate Input here
			var persons = _personRepository.FindPeople(userRoles.Persons);
			var roles = _roleRepository.LoadAll();
			var selectedRoles = roles.Where(x => userRoles.Roles.ToList().Contains(x.Id ?? Guid.Empty));

			foreach (var person in persons)
			{
				selectedRoles.ForEach(role => person.PermissionInformation.RemoveApplicationRole(role));
			}
		}
	}
}