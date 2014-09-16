using System;
using System.Linq;
using System.Web.Mvc;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[Intercept(typeof(AspectInterceptor))]
	public class StaffingMetricsController : Controller
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IDailyStaffingMetricsViewModelFactory _dailyStaffingMetricsViewModelFactory;

		public StaffingMetricsController(ISkillRepository skillRepository, IDailyStaffingMetricsViewModelFactory dailyStaffingMetricsViewModelFactory)
		{
			_skillRepository = skillRepository;
			_dailyStaffingMetricsViewModelFactory = dailyStaffingMetricsViewModelFactory;
		}

		[UnitOfWork(Order = 1), MultipleBusinessUnits(Order = 2), HttpGet]
		public virtual JsonResult AvailableSkills(DateTime date)
		{
			var dateOnly = new DateOnly(date);
			var skills = _skillRepository.FindAllWithSkillDays(new DateOnlyPeriod(dateOnly, dateOnly)).Select(s => new { s.Id, s.Name }).OrderBy(s => s.Name).ToList();
			return Json(new
				{
					Skills = skills
				}, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult DailyStaffingMetrics(Guid skillId, DateTime date)
		{
			var vm = _dailyStaffingMetricsViewModelFactory.CreateViewModel(skillId, date);
			return Json(vm, JsonRequestBehavior.AllowGet);
		}
	}
}