using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleDataController : ApiController
	{
		private readonly IPersonRepository _personRepo;
		private readonly ISkillRepository _skillRepo;

		public PeopleDataController(IPersonRepository personRepo, ISkillRepository skillRepo)
		{
			_personRepo = personRepo;
			_skillRepo = skillRepo;
		}

		[UnitOfWork]
		[HttpPost, Route("fetchPeople")]
		public JsonResult<IEnumerable<PersonDataModel>> FetchPeople(InputModel inputModel)
		{
			var people = _personRepo.FindPeople(inputModel.PersonIdList);
			var result = people.Select(p =>
			{
				var currentPeriod =
					p.PersonPeriods(new DateOnlyPeriod(new DateOnly(inputModel.Date), new DateOnly(inputModel.Date))).Single();
				return new PersonDataModel
				{
					PersonId = p.Id.GetValueOrDefault(),
					FirstName = p.Name.FirstName,
					LastName = p.Name.LastName,
					Team = currentPeriod.Team.SiteAndTeam,
					SkillIdList = currentPeriod.PersonSkillCollection.Select(s => s.Skill.Id.GetValueOrDefault()).ToList(),
					ShiftBag = currentPeriod.RuleSetBag.Description.Name
				};
			});

			return Json(result);
		}

		[UnitOfWork]
		[HttpGet, Route("loadAllSkills")]
		public JsonResult<IEnumerable<SkillDataModel>> LoadAllSkills()
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