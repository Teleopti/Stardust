using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core;
using Teleopti.Ccc.Web.Areas.People.Models;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	//[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport)]
	public class RoleController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly IApplicationRoleRepository _roleRepository;
		private readonly IRoleManager _roleManager;


		public RoleController(IPersonRepository personRepository,
			IApplicationRoleRepository roleRepository,
			IRoleManager roleManager)
		{
			_roleRepository = roleRepository;
			_personRepository = personRepository;
			_roleManager = roleManager;
		}

		[UnitOfWork, HttpGet, Route("api/PeopleData/fetchRoles")]
		public virtual IEnumerable<RoleViewModel> FetchRoles()
		{
			var roles = _roleRepository.LoadAllApplicationRolesSortedByName();

			var res = roles.Select(x => new RoleViewModel
			{
				Id = x.Id.GetValueOrDefault(),
				Name = x.DescriptionText,
				CanBeChangedByCurrentUser = true
			});

			return res;
		}

		[UnitOfWork, HttpPost, Route("api/PeopleData/fetchPersons"), RequireArguments]
		public virtual IEnumerable<PersonViewModel> FetchPersons(FecthPersonsInputModel users)
		{
			var persons = _personRepository.FindPeople(users.PersonIdList);

			var result = persons.Select(x => new PersonViewModel
			{
				Id = x.Id.GetValueOrDefault(),
				FirstName = x.Name.FirstName,
				LastName = x.Name.LastName,
				Roles = x.PermissionInformation.ApplicationRoleCollection.Select(r => new RoleViewModel
				{
					Name = r.DescriptionText,
					Id = r.Id.GetValueOrDefault(),
					CanBeChangedByCurrentUser = true
				})
			});

			return result;
		}

		[UnitOfWork, HttpPost, Route("api/PeopleCommand/grantRoles"), RequireArguments]
		public virtual IHttpActionResult GrantRoles(GrantRolesInputModel inputmodel)
		{
			_roleManager.GrantRoles(inputmodel);
			return Ok();
		}

		[UnitOfWork, HttpPost, Route("api/PeopleCommand/revokeRoles"), RequireArguments]
		public virtual IHttpActionResult RevokeRoles(RevokeRolesInputModel inputmodel)
		{
			_roleManager.RevokeRoles(inputmodel);
			return Ok();
		}
	}
}