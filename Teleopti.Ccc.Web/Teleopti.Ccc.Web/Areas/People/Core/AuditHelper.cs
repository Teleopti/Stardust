﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.People.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public interface IAuditHelper
	{
		void AuditCall(PersonAuditActionType actionType, PersonRolesBaseModel inputmodel);
	}

	public class AuditHelper : IAuditHelper
	{
		private readonly IPersonRepository _personRepository;
		private readonly IApplicationRoleRepository _roleRepository;
		private readonly ILoggedOnUser _loggonUser;
		private readonly IRepository<IPersonAccess> _personAccessRepository;
		private readonly IAuthorization _principalAuthorization;

		public AuditHelper(IPersonRepository personRepository,
			IApplicationRoleRepository roleRepository,
			ILoggedOnUser loggedOnUser,
			IRepository<IPersonAccess> personAccessRepository,
			IAuthorization principalAuthorization)

		{
			_roleRepository = roleRepository;
			_personRepository = personRepository;
			_loggonUser = loggedOnUser;
			_personAccessRepository = personAccessRepository;
			_principalAuthorization = principalAuthorization;
		}

		public void AuditCall(PersonAuditActionType actionType, PersonRolesBaseModel inputmodel)
		{
			actionType = tryTuneActionType(actionType, inputmodel);
			var persons = _personRepository.FindPeople(inputmodel.Persons);
			var updatingUser = _loggonUser.CurrentUser();
			var allRoles = _roleRepository.LoadAll();
			var selectedRoles = allRoles.Where(x => inputmodel.Roles.ToList().Contains(x.Id ?? Guid.Empty));
			var correlationId = Guid.NewGuid();

			foreach (var person in persons)
			{
				IEnumerable<IApplicationRole> rolesThatMakesChange;
				IEnumerable<IApplicationRole> rolesThatMakesNOChange;

				switch (actionType)
				{
					case PersonAuditActionType.MultiGrantRole:
					case PersonAuditActionType.SingleGrantRole:
						rolesThatMakesChange = selectedRoles.Except(person.PermissionInformation.ApplicationRoleCollection);
						rolesThatMakesNOChange = selectedRoles.Where(r => person.PermissionInformation.ApplicationRoleCollection.Contains(r));
						break;
					case PersonAuditActionType.MultiRevokeRole:
					case PersonAuditActionType.SingleRevokeRole:
					default:
						rolesThatMakesChange = selectedRoles.Where(r => person.PermissionInformation.ApplicationRoleCollection.Contains(r));
						rolesThatMakesNOChange = selectedRoles.Except(person.PermissionInformation.ApplicationRoleCollection);
						break;
				}
				if (!_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.PeopleAccess, DateOnly.Today, person))
				{
					persistAudit(actionType, rolesThatMakesNOChange, updatingUser, person, correlationId, PersonAuditActionResult.NotPermitted);
					persistAudit(actionType, rolesThatMakesChange, updatingUser, person, correlationId, PersonAuditActionResult.NotPermitted);
					continue;
				}
				persistAudit(actionType, rolesThatMakesNOChange, updatingUser, person, correlationId, PersonAuditActionResult.NoChange);
				persistAudit(actionType, rolesThatMakesChange, updatingUser, person, correlationId, PersonAuditActionResult.Change);
			}
		}

		private PersonAuditActionType tryTuneActionType(PersonAuditActionType actionType, PersonRolesBaseModel model)
		{
			switch (actionType)
			{
				case PersonAuditActionType.GrantRole:
					return model.Persons.Count() > 1 ? PersonAuditActionType.MultiGrantRole : PersonAuditActionType.SingleGrantRole;
				case PersonAuditActionType.RevokeRole:
					return model.Persons.Count() > 1 ? PersonAuditActionType.MultiRevokeRole : PersonAuditActionType.SingleRevokeRole;
				default:
					return actionType;
			}
		}

		private void persistAudit(PersonAuditActionType actionType, IEnumerable<IApplicationRole> rolesToUpdate, IPerson actionBy, IPerson actionOn, Guid correlationId, PersonAuditActionResult actionResult)
		{
			foreach (var role in rolesToUpdate)
			{
				var actionJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(new { RoleId = role.Id, Name = role.DescriptionText });
				var pa = new PersonAccess(actionBy, actionOn, actionType.ToString(), actionResult.ToString(), actionJsonData, correlationId);
				_personAccessRepository.Add(pa);
			}
		}
	}

	public enum PersonAuditActionResult
	{
		Change,
		NoChange,
		NotPermitted
	}

	public enum PersonAuditActionType
	{
		GrantRole,
		RevokeRole,
		SingleGrantRole,
		SingleRevokeRole,
		MultiGrantRole,
		MultiRevokeRole
	}
}