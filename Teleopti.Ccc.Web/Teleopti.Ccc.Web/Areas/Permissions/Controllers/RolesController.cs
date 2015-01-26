using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class RolesController : ApiController
	{
		private readonly IApplicationRoleRepository _roleRepository;
		private readonly IApplicationFunctionRepository _applicationFunctionRepository;
		private readonly IAvailableDataRepository _availableDataRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public RolesController(IApplicationRoleRepository roleRepository, IApplicationFunctionRepository applicationFunctionRepository, IAvailableDataRepository availableDataRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_roleRepository = roleRepository;
			_applicationFunctionRepository = applicationFunctionRepository;
			_availableDataRepository = availableDataRepository;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles"), HttpPost]
		public IHttpActionResult Post([FromBody] NewRoleInput model)
		{
			var role = createNewRole(model.Name);

			return Created(Request.RequestUri + role.Id.ToString(), new { role.Name, role.Id });
		}

		private IApplicationRole createNewRole(string name)
		{
			var role = new ApplicationRole {Name = name};
			role.SetBusinessUnit(_currentBusinessUnit.Current());
			_roleRepository.Add(role);

			var availableData = new AvailableData {ApplicationRole = role};
			_availableDataRepository.Add(availableData);

			role.AvailableData = availableData;

			return role;
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles"), HttpGet]
		public ICollection<object> Get()
		{
			var roles = _roleRepository.LoadAllApplicationRolesSortedByName();
			return roles.Select(r => new { r.Name, r.Id, r.BuiltIn }).ToArray();
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles/{roleId}"), HttpGet]
		public object Get(Guid roleId)
		{
			var role = _roleRepository.Get(roleId);
			return
				new
				{
					role.Name,
					role.Id,
					role.BuiltIn,
					role.AvailableData.AvailableDataRange,
					AvailableBusinessUnits = role.AvailableData.AvailableBusinessUnits.Select(b => new { b.Name, b.Id }).ToArray(),
					AvailableSites = role.AvailableData.AvailableSites.Select(s => new { s.Description.Name, s.Id }).ToArray(),
					AvailableTeams = role.AvailableData.AvailableTeams.Select(s => new { s.Description.Name, s.Id }).ToArray(),
					AvailablePeople = role.AvailableData.AvailablePersons.Select(s => new { s.Name, s.Id }).ToArray(),
				};
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles/{roleId}/Functions"),HttpPost]
		public void AddFunctions(Guid roleId, [FromBody]FunctionsForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;
			foreach (var function in model.Functions)
			{
				role.AddApplicationFunction(_applicationFunctionRepository.Load(function));
			}
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles/{roleId}"), HttpDelete]
		public void Delete(Guid roleId)
		{
			var role = _roleRepository.Load(roleId);
			if (!role.BuiltIn)
			{
				_roleRepository.Remove(role);
			}
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles/{roleId}/Functions"), HttpDelete]
		public void RemoveFunctions(Guid roleId, [FromBody]FunctionsForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;
			foreach (var function in model.Functions)
			{
				role.RemoveApplicationFunction(_applicationFunctionRepository.Load(function));
			}
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles/{roleId}"),HttpPut]
		public void RenameRole(Guid roleId, [FromBody]RoleNameInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;
			if (nameIsInvalid(model.NewName)) return;

			role.Name = model.NewName;
		}

		private bool nameIsInvalid(string newName)
		{
			return string.IsNullOrEmpty(newName);
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles/{roleId}/Copy"),HttpPost]
		public void CopyExistingRole(Guid roleId, [FromBody]RoleNameInput model)
		{
			var newRole = createNewRole(model.NewName);

			var roleToCopy = _roleRepository.Get(roleId);
			roleToCopy.ApplicationFunctionCollection.ForEach(newRole.AddApplicationFunction);
			roleToCopy.AvailableData.AvailableBusinessUnits.ForEach(newRole.AvailableData.AddAvailableBusinessUnit);
			roleToCopy.AvailableData.AvailableSites.ForEach(newRole.AvailableData.AddAvailableSite);
			roleToCopy.AvailableData.AvailableTeams.ForEach(newRole.AvailableData.AddAvailableTeam);
			roleToCopy.AvailableData.AvailablePersons.ForEach(newRole.AvailableData.AddAvailablePerson);
			newRole.AvailableData.AvailableDataRange = roleToCopy.AvailableData.AvailableDataRange;
		}
	}
}