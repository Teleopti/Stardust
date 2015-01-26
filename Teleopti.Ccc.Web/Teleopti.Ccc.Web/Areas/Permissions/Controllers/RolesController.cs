using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		[UnitOfWork]
		[Route("api/Permissions/Roles"), HttpPost]
		public IHttpActionResult Post([FromBody] NewRoleInput model)
		{
			if (descriptionIsInvalid(model.Description)) return BadRequest("The description was invalid. It can contain at most 255 characters.");
			var role = createNewRole(model.Description);

			return Created(Request.RequestUri + role.Id.ToString(), new { role.Name, role.Id, role.DescriptionText });
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
					AvailableTeams = role.AvailableData.AvailableTeams.Select(s => new { s.Description.Name, s.Id }).ToArray(),
					AvailablePeople = role.AvailableData.AvailablePersons.Select(s => new { s.Name, s.Id }).ToArray(),
				};
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/Functions"),HttpPost]
		public virtual void AddFunctions(Guid roleId, [FromBody]FunctionsForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;
			foreach (var function in model.Functions)
			{
				role.AddApplicationFunction(_applicationFunctionRepository.Load(function));
			}
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}"), HttpDelete]
		public virtual void Delete(Guid roleId)
		{
			var role = _roleRepository.Load(roleId);
			if (!role.BuiltIn)
			{
				_roleRepository.Remove(role);
			}
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/Functions"), HttpDelete]
		public virtual void RemoveFunctions(Guid roleId, [FromBody]FunctionsForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;
			foreach (var function in model.Functions)
			{
				role.RemoveApplicationFunction(_applicationFunctionRepository.Load(function));
			}
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}"),HttpPut]
		public virtual void RenameRole(Guid roleId, [FromBody]RoleNameInput model)
		{
			if (descriptionIsInvalid(model.NewDescription)) return;

			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;

			role.DescriptionText = model.NewDescription;
			role.Name = descriptionToName(model.NewDescription);
		}

		private bool descriptionIsInvalid(string newDescription)
		{
			return string.IsNullOrEmpty(newDescription) || newDescription.Length>255;
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/Copy"),HttpPost]
		public virtual void CopyExistingRole(Guid roleId, [FromBody]RoleNameInput model)
		{
			if (descriptionIsInvalid(model.NewDescription)) return;

			var newRole = createNewRole(model.NewDescription);

			var roleToCopy = _roleRepository.Get(roleId);
			roleToCopy.ApplicationFunctionCollection.ForEach(newRole.AddApplicationFunction);
			roleToCopy.AvailableData.AvailableBusinessUnits.ForEach(newRole.AvailableData.AddAvailableBusinessUnit);
			roleToCopy.AvailableData.AvailableSites.ForEach(newRole.AvailableData.AddAvailableSite);
			roleToCopy.AvailableData.AvailableTeams.ForEach(newRole.AvailableData.AddAvailableTeam);
			roleToCopy.AvailableData.AvailablePersons.ForEach(newRole.AvailableData.AddAvailablePerson);
			newRole.AvailableData.AvailableDataRange = roleToCopy.AvailableData.AvailableDataRange;
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/AvailableData"), HttpPost]
		public virtual void AddAvailableData(Guid roleId, [FromBody]AvailableDataForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;

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
			model.People.ForEach(x =>
			{
				var person = new Person();
				person.SetId(x);
				role.AvailableData.AddAvailablePerson(person);
			});
			role.AvailableData.AvailableDataRange = model.RangeOption.GetValueOrDefault(role.AvailableData.AvailableDataRange);
		}

		[UnitOfWork, Route("api/Permissions/Roles/{roleId}/AvailableData"), HttpDelete]
		public virtual void RemoveAvailableData(Guid roleId, [FromBody]AvailableDataForRoleInput model)
		{
			var role = _roleRepository.Get(roleId);
			if (role.BuiltIn) return;

			var businessUnits =
				role.AvailableData.AvailableBusinessUnits.Where(b => model.BusinessUnits.Contains(b.Id.GetValueOrDefault())).ToArray();
			businessUnits.ForEach(role.AvailableData.DeleteAvailableBusinessUnit);
			var sites =
				role.AvailableData.AvailableSites.Where(s => model.Sites.Contains(s.Id.GetValueOrDefault())).ToArray();
			sites.ForEach(role.AvailableData.DeleteAvailableSite);
			var teams =
				role.AvailableData.AvailableTeams.Where(t => model.Teams.Contains(t.Id.GetValueOrDefault())).ToArray();
			teams.ForEach(role.AvailableData.DeleteAvailableTeam);
			var people =
				role.AvailableData.AvailablePersons.Where(p => model.People.Contains(p.Id.GetValueOrDefault())).ToArray();
			people.ForEach(role.AvailableData.DeleteAvailablePerson);
		}
	}
}