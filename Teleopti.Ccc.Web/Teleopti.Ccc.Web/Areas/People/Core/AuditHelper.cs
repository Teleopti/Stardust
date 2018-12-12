using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.Web.Areas.People.Models;


namespace Teleopti.Ccc.Web.Areas.People.Core
{
	

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
					case PersonAuditActionType.AuditTrailMultiGrantRole:
					case PersonAuditActionType.AuditTrailSingleGrantRole:
						rolesThatMakesChange = selectedRoles.Except(person.PermissionInformation.ApplicationRoleCollection);
						rolesThatMakesNOChange = selectedRoles.Where(r => person.PermissionInformation.ApplicationRoleCollection.Contains(r));
						break;
					case PersonAuditActionType.AuditTrailMultiRevokeRole:
					case PersonAuditActionType.AuditTrailSingleRevokeRole:
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
					return model.Persons.Count() > 1 ? PersonAuditActionType.AuditTrailMultiGrantRole : PersonAuditActionType.AuditTrailSingleGrantRole;
				case PersonAuditActionType.RevokeRole:
					return model.Persons.Count() > 1 ? PersonAuditActionType.AuditTrailMultiRevokeRole : PersonAuditActionType.AuditTrailSingleRevokeRole;
				default:
					return actionType;
			}
		}

		private void persistAudit(PersonAuditActionType actionType, IEnumerable<IApplicationRole> rolesToUpdate, IPerson actionBy, IPerson actionOn, Guid correlationId, PersonAuditActionResult actionResult)
		{
			foreach (var role in rolesToUpdate)
			{
				var actionJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(new { RoleId = role.Id, Name = role.DescriptionText });
				//this will be a comma seperated values of searchable strings
				var searchKeys = role.DescriptionText;
				var pa = new PersonAccess(actionBy, actionOn, actionType.ToString(), actionResult.ToString(), actionJsonData, searchKeys, correlationId);
				_personAccessRepository.Add(pa);
			}
		}
	}
}