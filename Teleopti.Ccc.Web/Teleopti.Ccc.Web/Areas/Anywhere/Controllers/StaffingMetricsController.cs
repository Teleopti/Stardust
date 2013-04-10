using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class StaffingMetricsController : Controller
	{
		private readonly ISkillRepository _skillRepository;

		public StaffingMetricsController(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult AvailableSkills(DateTime date)
		{
			var dateOnly = new DateOnly(date);
			var skills = _skillRepository.FindAllWithSkillDays(new DateOnlyPeriod(dateOnly, dateOnly)).Select(s => new { s.Id, s.Name }).OrderBy(s => s.Name).ToList();
			return Json(new
				{
					Skills = skills
				}, JsonRequestBehavior.AllowGet);
		}
	}
}