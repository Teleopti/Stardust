using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class StaffingMetricsController : ApiController
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IDailyStaffingMetricsViewModelFactory _dailyStaffingMetricsViewModelFactory;

		public StaffingMetricsController(ISkillRepository skillRepository, IDailyStaffingMetricsViewModelFactory dailyStaffingMetricsViewModelFactory)
		{
			_skillRepository = skillRepository;
			_dailyStaffingMetricsViewModelFactory = dailyStaffingMetricsViewModelFactory;
		}

		[UnitOfWork, HttpGet, Route("api/StaffingMetrics/AvailableSkills")]
		public virtual IHttpActionResult AvailableSkills(DateTime date)
		{
			var dateOnly = new DateOnly(date);
			var skills = _skillRepository.FindAllWithSkillDays(new DateOnlyPeriod(dateOnly, dateOnly)).Select(s => new AvailableSkillModel(s.Id, s.Name)).OrderBy(s => s.Name).ToList();
			var content = new AvailableSkillsModel
			{
				Skills = skills
			};
			return Ok(content);
		}

		[UnitOfWork, HttpGet, Route("api/StaffingMetrics/DailyStaffingMetrics")]
		public virtual IHttpActionResult DailyStaffingMetrics(Guid skillId, DateTime date)
		{
			var vm = _dailyStaffingMetricsViewModelFactory.CreateViewModel(skillId, date);
			return Ok(vm);
		}
	}

	public class AvailableSkillsModel
	{
		public ICollection<AvailableSkillModel> Skills { get; set; }
	}

	public class AvailableSkillModel
	{
		public AvailableSkillModel(Guid? id, string name)
		{
			Id = id;
			Name = name;
		}

		public Guid? Id { get; set; }
		public string Name { get; set; }
	}
}