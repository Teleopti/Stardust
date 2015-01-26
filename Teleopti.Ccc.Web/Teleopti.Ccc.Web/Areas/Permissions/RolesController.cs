using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Permissions
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
			var role = new ApplicationRole { Name = model.Name };
			role.SetBusinessUnit(_currentBusinessUnit.Current());
			_roleRepository.Add(role);

			var availableData = new AvailableData { ApplicationRole = role };
			_availableDataRepository.Add(availableData);

			return Created(Request.RequestUri + role.Id.ToString(), new { role.Name, role.Id });
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
			_roleRepository.Remove(role);
		}

		[UnitOfWorkApiAction]
		[Route("api/Permissions/Roles/{roleId}/Functions"), HttpDelete]
		public void RemoveFunctions(Guid roleId, [FromBody]FunctionsForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			foreach (var function in model.Functions)
			{
				role.RemoveApplicationFunction(_applicationFunctionRepository.Load(function));
			}
		}
	}

	public struct FunctionsForRoleInput
	{
		public ICollection<Guid> Functions { get; set; }
	}
}