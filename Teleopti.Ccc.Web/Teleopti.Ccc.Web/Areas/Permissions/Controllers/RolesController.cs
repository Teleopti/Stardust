using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.SignalR.Infrastructure;
using NodaTime.TimeZones;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
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
        private const string GivenDescriptionIsInvalidErrorMessage = "The given description is invalid. It can contain at most 255 characters.";
        private const string CannotModifyBuiltInRoleErrorMessage = "Roles that are built in cannot be changed.";

        private readonly IApplicationRoleRepository _roleRepository;
        private readonly IApplicationFunctionRepository _applicationFunctionRepository;
        private readonly IAvailableDataRepository _availableDataRepository;
        private readonly ICurrentBusinessUnit _currentBusinessUnit;
	    private readonly IBusinessUnitRepository _businessUnitRepository;
	    private readonly ISiteRepository _siteRepository;
	    private readonly ITeamRepository _teamRepository;
	 

        public RolesController(IApplicationRoleRepository roleRepository, IApplicationFunctionRepository applicationFunctionRepository, IAvailableDataRepository availableDataRepository, ICurrentBusinessUnit currentBusinessUnit, IBusinessUnitRepository businessUnitRepository, ISiteRepository siteRepository, ITeamRepository teamRepository)
        {
            _roleRepository = roleRepository;
            _applicationFunctionRepository = applicationFunctionRepository;
            _availableDataRepository = availableDataRepository;
            _currentBusinessUnit = currentBusinessUnit;
	        _businessUnitRepository = businessUnitRepository;
	        _siteRepository = siteRepository;
	        _teamRepository = teamRepository;
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
            var role = new ApplicationRole { DescriptionText = description, Name = descriptionToName(description) };
            role.SetBusinessUnit(_currentBusinessUnit.Current());
            _roleRepository.Add(role);

            var availableData = new AvailableData { ApplicationRole = role };
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

        [UnitOfWork, Route("api/Permissions/Roles/{roleId}/Functions"), HttpPost]
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

        [UnitOfWork, Route("api/Permissions/Roles/{roleId}/Function/{functionId}"), HttpDelete]
        public virtual IHttpActionResult RemoveFunction(Guid roleId, Guid functionId)
        {
            var role = _roleRepository.Get(roleId);
            if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);
            var children = _applicationFunctionRepository.GetChildFunctions(functionId);
            foreach (var child in children)
                if (child.Id.HasValue) removeChildren(child.Id.Value, role);

            role.RemoveApplicationFunction(_applicationFunctionRepository.Load(functionId));
            return Ok();
        }

		 [UnitOfWork, Route("api/Permissions/Roles/{roleId}/DeleteFunction/{functionId}"), HttpPost]
		  public virtual IHttpActionResult RemoveFunction(Guid roleId, Guid functionId, [FromBody]FunctionsForRoleInput parents)
		  {
			  var role = _roleRepository.Get(roleId);
			  if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);
			  var children = _applicationFunctionRepository.GetChildFunctions(functionId);
			  foreach (var child in children)
				  if (child.Id.HasValue) removeChildren(child.Id.Value, role);

			  role.RemoveApplicationFunction(_applicationFunctionRepository.Load(functionId));
			  parents.Functions.ForEach(x => role.RemoveApplicationFunction(_applicationFunctionRepository.Load(x)));
			  return Ok();
		  }
		 
        private void removeChildren(Guid functionId, IApplicationRole role)
        {
            var functionIds = _applicationFunctionRepository.GetChildFunctions(functionId);
            foreach (var child in functionIds)
            {
                if (!child.Id.HasValue) continue;
                if (_applicationFunctionRepository.GetChildFunctions(child.Id.Value).Any())
                    removeChildren(child.Id.Value, role);
                role.RemoveApplicationFunction(_applicationFunctionRepository.Load(child.Id.Value));
            }
            role.RemoveApplicationFunction(_applicationFunctionRepository.Load(functionId));
        }

        [UnitOfWork, Route("api/Permissions/Roles/{roleId}"), HttpPut]
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
            return string.IsNullOrEmpty(newDescription) || newDescription.Length > 255;
        }

        [UnitOfWork, Route("api/Permissions/Roles/{roleId}/Copy"), HttpPost]
        public virtual IHttpActionResult CopyExistingRole(Guid roleId)
        {
            var roleToCopy = _roleRepository.Get(roleId);
            var newRole = createNewRole(string.Format(CultureInfo.CurrentUICulture, "{0} {1}", UserTexts.Resources.CopyOf, roleToCopy.DescriptionText));
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

            model.BusinessUnits.ForEach(x => role.AvailableData.AddAvailableBusinessUnit(_businessUnitRepository.Load(x)));
            model.Sites.ForEach(x => role.AvailableData.AddAvailableSite(_siteRepository.Load(x)));
            model.Teams.ForEach(x => role.AvailableData.AddAvailableTeam(_teamRepository.Load(x)));
            role.AvailableData.AvailableDataRange = model.RangeOption.GetValueOrDefault(role.AvailableData.AvailableDataRange);

            return Ok();
        }

        [UnitOfWork, Route("api/Permissions/Roles/{roleId}/AvailableData/BusinessUnit/{businessUnitId}"), HttpDelete]
        public virtual IHttpActionResult RemoveAvailableBusinessUnit(Guid roleId, Guid businessUnitId)
        {
            var role = _roleRepository.Get(roleId);
            if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);

            var businessUnits = role.AvailableData.AvailableBusinessUnits.Where(b => businessUnitId == b.Id.GetValueOrDefault()).ToArray();
            businessUnits.ForEach(businessUnit =>
            {
                var sites = role.AvailableData.AvailableSites.Where(s => businessUnit.Id.GetValueOrDefault() == s.BusinessUnit.Id.GetValueOrDefault()).ToArray();
                sites.ForEach(site =>
                {
                    var teams = role.AvailableData.AvailableTeams.Where(t => site.Id.GetValueOrDefault() == t.Site.Id.GetValueOrDefault()).ToArray();
                    teams.ForEach(role.AvailableData.DeleteAvailableTeam);
                });
                sites.ForEach(role.AvailableData.DeleteAvailableSite);
            });
            businessUnits.ForEach(role.AvailableData.DeleteAvailableBusinessUnit);

            return Ok();
        }

        [UnitOfWork, Route("api/Permissions/Roles/{roleId}/AvailableData/Team/{teamId}"), HttpDelete]
        public virtual IHttpActionResult RemoveAvailableTeam(Guid roleId, Guid teamId)
        {
            var role = _roleRepository.Get(roleId);
            if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);

            var teams = role.AvailableData.AvailableTeams.Where(t => teamId == t.Id.GetValueOrDefault()).ToArray();
            teams.ForEach(role.AvailableData.DeleteAvailableTeam);

            return Ok();
        }


        [UnitOfWork, Route("api/Permissions/Roles/{roleId}/AvailableData/Site/{siteId}"), HttpDelete]
        public virtual IHttpActionResult RemoveAvailableSite(Guid roleId, Guid siteId)
        {
            var role = _roleRepository.Get(roleId);
            if (role.BuiltIn) return BadRequest(CannotModifyBuiltInRoleErrorMessage);

            var sites = role.AvailableData.AvailableSites.Where(s => siteId == s.Id.GetValueOrDefault()).ToArray();
            sites.ForEach(site =>
            {
                var teams = role.AvailableData.AvailableTeams.Where(t => siteId == t.Site.Id.GetValueOrDefault()).ToArray();
                teams.ForEach(role.AvailableData.DeleteAvailableTeam);
            });
            sites.ForEach(role.AvailableData.DeleteAvailableSite);

            return Ok();
        }
    }
}
