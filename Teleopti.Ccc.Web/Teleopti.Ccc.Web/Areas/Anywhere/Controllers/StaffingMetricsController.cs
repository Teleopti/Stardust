using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class StaffingMetricsController : Controller
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IDailyStaffingMetricsViewModelFactory _dailyStaffingMetricsViewModelFactory;

		public StaffingMetricsController(ISkillRepository skillRepository, IDailyStaffingMetricsViewModelFactory dailyStaffingMetricsViewModelFactory)
		{
			_skillRepository = skillRepository;
			_dailyStaffingMetricsViewModelFactory = dailyStaffingMetricsViewModelFactory;
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult AvailableSkills(DateTime date)
		{
			var dateOnly = new DateOnly(date);
			var skills = _skillRepository.FindAllWithSkillDays(new DateOnlyPeriod(dateOnly, dateOnly)).Select(s => new { s.Id, s.Name }).OrderBy(s => s.Name).ToList();
			return Json(new
				{
					Skills = skills
				}, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult DailyStaffingMetrics(Guid skillId, DateTime date)
		{
			var vm = _dailyStaffingMetricsViewModelFactory.CreateViewModel(skillId, date);
			return Json(vm, JsonRequestBehavior.AllowGet);
		}
	}
}