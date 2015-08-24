using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleDataController : ApiController
	{
		private readonly IPersonDataProvider _personDataProvider;
		private readonly ISkillRepository _skillRepo;

		public PeopleDataController(IPersonDataProvider personDataProvider, ISkillRepository skillRepo)
		{
			_personDataProvider = personDataProvider;
			_skillRepo = skillRepo;
		}

		[UnitOfWork, HttpPost, Route("api/PeopleData/fetchPeople")]
		public virtual JsonResult<IEnumerable<PersonDataModel>> FetchPeople(InputModel inputModel)
		{
			var result = _personDataProvider.RetrievePeople(inputModel.Date, inputModel.PersonIdList);
			
			return Json(result);
		}

		[UnitOfWork, HttpGet, Route("api/PeopleData/loadAllSkills")]
		public virtual JsonResult<IEnumerable<SkillDataModel>> LoadAllSkills()
		{
			var skills = _skillRepo.LoadAll();
			var result = skills.Select(s => new SkillDataModel
			{
				SkillId = s.Id.GetValueOrDefault(),
				SkillName = s.Name
			});
			return Json(result);
		}
	}

	public class SkillDataModel
	{
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
	}

	public class PersonDataModel
	{
		public Guid PersonId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Team { get; set; }
		public IList<Guid> SkillIdList { get; set; }
		public string ShiftBag { get; set; }
	}

	public class InputModel
	{
		public DateTime Date { get; set; }
		public IEnumerable<Guid> PersonIdList { get; set; }
	}
}