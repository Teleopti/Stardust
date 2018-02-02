using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Models;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class RoleController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly IApplicationRoleRepository _roleRepository;

		public RoleController(IPersonRepository personRepository, IApplicationRoleRepository roleRepository)
		{
			_roleRepository = roleRepository;
			_personRepository = personRepository;
		}

		[UnitOfWork, HttpGet, Route("api/PeopleData/fetchRoles")]
		public virtual JsonResult<IEnumerable<RoleViewModel>> FetchRoles()
		{
			var roles = _roleRepository.LoadAllApplicationRolesSortedByName();

			var res = roles.Select(x => new RoleViewModel
			{
				Id = x.Id.GetValueOrDefault(),
				Name = x.Name,
				CanBeChangedByCurrentUser = true
			});

			return Json(res);
		}

		[UnitOfWork, HttpPost, Route("api/PeopleData/fetchPersons")]
		public virtual JsonResult<IEnumerable<PersonViewModel>> FetchPersons(FecthPersonsInputModel users)
		{
			var persons = _personRepository.FindPeople(users.PersonIdList);

			var result = persons.Select(x => new PersonViewModel
			{
				Id = x.Id.GetValueOrDefault(),
				FirstName = x.Name.FirstName,
				LastName = x.Name.LastName,
				Roles = x.PermissionInformation.ApplicationRoleCollection.Select(r => new RoleViewModel
				{
					Name = r.Name,
					Id = r.Id.GetValueOrDefault(),
					CanBeChangedByCurrentUser = true
				})
			});

			return Json(result);
		}

		[UnitOfWork, HttpPost, Route("api/PeopleCommand/grantRoles")]
		public virtual JsonResult<ResultViewModel> GrantRoles(GrantRolesInputModel inputmodel)
		{
			var result = new ResultViewModel();
			try
			{
				result.SuccessCount = 1; 
				result.Success = true;
			}
			catch (Exception e)
			{
				result.Success = false;
				result.ErrorMsg = e.Message;
			}

			return Json(result);
		}

		[UnitOfWork, HttpPost, Route("api/PeopleCommand/revokeRoles")]
		public virtual JsonResult<ResultViewModel> RevokeRoles(RevokeRolesInputModel inputmodel)
		{ 
			var result = new ResultViewModel();
			try
			{
				result.SuccessCount = 1;
				result.Success = true;
			}
			catch (Exception e)
			{
				result.Success = false;
				result.ErrorMsg = e.Message;
			}

			return Json(result);
		}
	}
}