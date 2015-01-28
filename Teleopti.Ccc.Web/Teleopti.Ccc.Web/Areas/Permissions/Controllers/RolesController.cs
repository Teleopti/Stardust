using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class RolesController : ApiController
	{
		private const string GivenDescriptionIsInvalidErrorMessage = "The given description is invalid. It can contain at most 255 characters.";
		private const string CannotModifyBuiltInRoleErrorMessage = "Roles that are built in cannot be changed.";

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

		[UnitOfWork, Route("api/Permissions/Roles"), HttpPost]
		public virtual IHttpActionResult Post([FromBody] NewRoleInput model)
		{
			if (descriptionIsInvalid(model.Description)) return BadRequest(GivenDescriptionIsInvalidErrorMessage);
			var role = createNewRole(model.Description);

			return Created(Request.RequestUri + "/" + role.Id, new { role.Name, role.Id, role.DescriptionText });
		}

		private IApplicationRole createNewRole(string description)
		{
			var role = new ApplicationRole {DescriptionText = description,Name = descriptionToName(description)};
			role.SetBusinessUnit(_currentBusinessUnit.Current());
			_roleRepository.Add(role);

			var availableData = new AvailableData {ApplicationRole = role};
			_availableDataRepository.Add(availableData);

			role.AvailableData = availableData;

			return role;
		}

		private string descriptionToName(string description)
		{
			var name = description.Replace(" ", string.Empty);
			if (name.Length > 50) name = name.Substring(0, 50);
			return name;
		}

		[UnitOfWork, Route("api/Permissions/Roles"), HttpGet]
		public virtual ICollection<object> Get()
		{
			var roles = _roleRepository.LoadAllApplicationRolesSortedByName();
			return roles.Select(r => new { r.Name, r.Id, r.BuiltIn, r.DescriptionText }).ToArray();
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}"), HttpGet]
		public virtual object Get(Guid roleId)
		{
			var role = _roleRepository.Get(roleId);
			return new
				{
					role.Name,
					role.Id,
					role.BuiltIn,
					role.DescriptionText,
					role.AvailableData.AvailableDataRange,
					AvailableBusinessUnits = role.AvailableData.AvailableBusinessUnits.Select(b => new { b.Name, b.Id }).ToArray(),
					AvailableSites = role.AvailableData.AvailableSites.Select(s => new { s.Description.Name, s.Id }).ToArray(),
					AvailableTeams = role.AvailableData.AvailableTeams.Select(t => new { t.Description.Name, t.Id }).ToArray(),
					AvailableFunctions = role.ApplicationFunctionCollection.Select(f => new { f.Id, f.FunctionCode, f.FunctionPath, f.LocalizedFunctionDescription }).ToArray()
				};
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/Functions"),HttpPost]
		public virtual IHttpActionResult AddFunctions(Guid roleId, [FromBody]FunctionsForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);
			foreach (var function in model.Functions)
			{
				role.AddApplicationFunction(_applicationFunctionRepository.Load(function));
			}
			return Ok();
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}"), HttpDelete]
		public virtual IHttpActionResult Delete(Guid roleId)
		{
			var role = _roleRepository.Load(roleId);
			if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);
			
			_roleRepository.Remove(role);
			return Ok();
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/Functions"), HttpDelete]
		public virtual IHttpActionResult RemoveFunctions(Guid roleId, [FromBody]FunctionsForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);

			foreach (var function in model.Functions)
			{
				role.RemoveApplicationFunction(_applicationFunctionRepository.Load(function));
			}
			return Ok();
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}"),HttpPut]
		public virtual IHttpActionResult RenameRole(Guid roleId, [FromBody]RoleNameInput model)
		{
			if (descriptionIsInvalid(model.NewDescription)) return BadRequest(GivenDescriptionIsInvalidErrorMessage);

			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);

			role.DescriptionText = model.NewDescription;
			role.Name = descriptionToName(model.NewDescription);

			return Ok();
		}

		private bool descriptionIsInvalid(string newDescription)
		{
			return string.IsNullOrEmpty(newDescription) || newDescription.Length>255;
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/Copy"),HttpPost]
		public virtual IHttpActionResult CopyExistingRole(Guid roleId, [FromBody]RoleNameInput model)
		{
			if (descriptionIsInvalid(model.NewDescription)) return BadRequest(GivenDescriptionIsInvalidErrorMessage);

			var newRole = createNewRole(model.NewDescription);

			var roleToCopy = _roleRepository.Get(roleId);
			roleToCopy.ApplicationFunctionCollection.ForEach(newRole.AddApplicationFunction);
			roleToCopy.AvailableData.AvailableBusinessUnits.ForEach(newRole.AvailableData.AddAvailableBusinessUnit);
			roleToCopy.AvailableData.AvailableSites.ForEach(newRole.AvailableData.AddAvailableSite);
			roleToCopy.AvailableData.AvailableTeams.ForEach(newRole.AvailableData.AddAvailableTeam);
			newRole.AvailableData.AvailableDataRange = roleToCopy.AvailableData.AvailableDataRange;

			return Created(Request.RequestUri + "/" + newRole.Id, new { newRole.Name, newRole.Id, newRole.DescriptionText });
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/AvailableData"), HttpPost]
		public virtual IHttpActionResult AddAvailableData(Guid roleId, [FromBody]AvailableDataForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);

			model.BusinessUnits.ForEach(x =>
			{
				var businessUnit = new BusinessUnit("temp");
				businessUnit.SetId(x);
				role.AvailableData.AddAvailableBusinessUnit(businessUnit);
			});
			model.Sites.ForEach(x =>
			{
				var site = new Site("temp");
				site.SetId(x);
				role.AvailableData.AddAvailableSite(site);
			});
			model.Teams.ForEach(x =>
			{
				var team = new Team();
				team.SetId(x);
				role.AvailableData.AddAvailableTeam(team);
			});
			role.AvailableData.AvailableDataRange = model.RangeOption.GetValueOrDefault(role.AvailableData.AvailableDataRange);

			return Ok();
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/AvailableData"), HttpDelete]
		public virtual IHttpActionResult RemoveAvailableData(Guid roleId, [FromBody]AvailableDataForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);

			var businessUnits =
				role.AvailableData.AvailableBusinessUnits.Where(b => model.BusinessUnits.Contains(b.Id.GetValueOrDefault())).ToArray();
			businessUnits.ForEach(role.AvailableData.DeleteAvailableBusinessUnit);
			var sites =
				role.AvailableData.AvailableSites.Where(s => model.Sites.Contains(s.Id.GetValueOrDefault())).ToArray();
			sites.ForEach(role.AvailableData.DeleteAvailableSite);
			var teams =
				role.AvailableData.AvailableTeams.Where(t => model.Teams.Contains(t.Id.GetValueOrDefault())).ToArray();
			teams.ForEach(role.AvailableData.DeleteAvailableTeam);

			return Ok();
		}
	}
}