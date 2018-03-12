using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core.Aspects;
using Teleopti.Ccc.Web.Areas.People.Models;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public interface IRoleManager
	{
		void GrantRoles(GrantRolesInputModel grantModel);
		void RevokeRoles(RevokeRolesInputModel revokeModel);
	}

	public class RoleManager : IRoleManager
	{
		private readonly IPersonRepository personRepository;
		private readonly IApplicationRoleRepository roleRepository;
		private readonly ILoggedOnUser loggedOnUser;

		public RoleManager(IPersonRepository personRepository,
			IApplicationRoleRepository roleRepository,
			ILoggedOnUser loggedOnUser)
		{
			this.roleRepository = roleRepository;
			this.personRepository = personRepository;
			this.loggedOnUser = loggedOnUser;
		}

		[AuditPerson]
		public virtual void GrantRoles(GrantRolesInputModel grantModel)
		{
			var persons = personRepository.FindPeople(grantModel.Persons);
			var allRoles = roleRepository.LoadAll();
			var selectedRoles = allRoles.Where(x => grantModel.Roles.ToList().Contains(x.Id ?? Guid.Empty));

			foreach (var person in persons)
			{
				selectedRoles.ForEach(role => person.PermissionInformation.AddApplicationRole(role));
			}
		}

		[AuditPerson]
		public virtual void RevokeRoles(RevokeRolesInputModel revokeModel)
		{
			var persons = personRepository.FindPeople(revokeModel.Persons);
			var allRoles = roleRepository.LoadAll();
			var selectedRoles = allRoles.Where(x => revokeModel.Roles.ToList().Contains(x.Id ?? Guid.Empty));

			foreach (var person in persons)
			{
				selectedRoles.ForEach(role => person.PermissionInformation.RemoveApplicationRole(role));
			}
		}
	}
}