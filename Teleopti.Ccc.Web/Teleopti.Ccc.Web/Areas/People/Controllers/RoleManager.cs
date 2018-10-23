﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.People.Core.Aspects;
using Teleopti.Interfaces.Domain;

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
		private readonly IAuthorization principalAuthorization;

		public RoleManager(IPersonRepository personRepository,
						   IApplicationRoleRepository roleRepository,
						   IAuthorization principalAuthorization)

		{
			this.roleRepository = roleRepository;
			this.personRepository = personRepository;
			this.principalAuthorization = principalAuthorization;
		}

		[AuditPerson]
		//[AuditTrail]
		public virtual void GrantRoles(GrantRolesInputModel grantModel)
		{
			var persons = personRepository.FindPeople(grantModel.Persons);
			var allRoles = roleRepository.LoadAll();
			var selectedRoles = allRoles.Where(x => grantModel.Roles.ToList().Contains(x.Id ?? Guid.Empty));

			foreach (var person in persons)
			{
				if (!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.PeopleAccess, DateOnly.Today, person))
				{
					continue;
				}
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
				if (!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.PeopleAccess, DateOnly.Today, person))
				{
					continue;
				}
				selectedRoles.ForEach(role => person.PermissionInformation.RemoveApplicationRole(role));
			}
		}
	}
}