using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkill)]
	public class SkillController : ApiController
	{
		private readonly IActivityProvider _activityProvider;
		private readonly ISkillRepository _skillRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IQueueSourceRepository _queueSourceRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillTypeProvider _skillTypeProvider;
		private readonly IWorkloadRepository _workloadRepository;

		public SkillController(IActivityProvider activityProvider, ISkillRepository skillRepository, IIntervalLengthFetcher intervalLengthFetcher, IQueueSourceRepository queueSourceRepository, IActivityRepository activityRepository, ISkillTypeProvider skillTypeProvider, IWorkloadRepository workloadRepository)
		{
			_activityProvider = activityProvider;
			_skillRepository = skillRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_queueSourceRepository = queueSourceRepository;
			_activityRepository = activityRepository;
			_skillTypeProvider = skillTypeProvider;
			_workloadRepository = workloadRepository;
		}

		[UnitOfWork, Route("api/Skill/Activities"), HttpGet]
		public virtual IEnumerable<dynamic> Activities()
		{
			return _activityProvider.GetAllRequireSkill().Select(activity => new {Id = activity.Id.GetValueOrDefault(), activity.Name});
		}

		[UnitOfWork, Route("api/Skill/Queues"), HttpGet]
		public virtual IEnumerable<dynamic> Queues()
		{
			return _queueSourceRepository.LoadAll()
				.Select(x => new {Id = x.Id.GetValueOrDefault(), x.Name, x.LogObjectName, x.Description});
		}

		[UnitOfWork, Route("api/Skill/Create"), HttpPost]
		public virtual IHttpActionResult Create(SkillInput input)
		{
			if (string.IsNullOrEmpty(input.Name))
				return BadRequest("Bad skill name");
			var skills = _skillRepository.LoadAll();
			var skill = skills.FirstOrDefault(x => x.Activity.Id == input.ActivityId);
			var intervalLength = skill?.DefaultResolution ?? _intervalLengthFetcher.GetIntervalLength();
			var newSkill = new Skill(input.Name, "", Color.Azure, intervalLength, _skillTypeProvider.InboundTelephony())
			{
				TimeZone = TimeZoneInfo.FindSystemTimeZoneById(input.TimezoneId),
				Activity = _activityRepository.Load(input.ActivityId)
			};
			var newWorkload = new Workload(newSkill)
			{
				Name = input.Name
			};

			SetOpenHours(newWorkload, input.OpenHours);
			SetSkillTemplates(newSkill, input.ServiceLevelPercent, input.ServiceLevelSeconds, input.Shrinkage);

			var queues = _queueSourceRepository.LoadAll();
			foreach (var queue in input.Queues)
			{
				var found = queues.SingleOrDefault(x => x.Id == queue);
				if (found != null)
					newWorkload.AddQueueSource(found);
			}

			newSkill.AddWorkload(newWorkload);
			_skillRepository.Add(newSkill);
			_workloadRepository.Add(newWorkload);
			return Ok(new { WorkloadId = newWorkload.Id });
		}

		private void SetSkillTemplates(ISkill skill, double serviceLevelPercent, double serviceLevelSecond, double shrinkagePercent)
		{
			const double minOccupancy = 0.3;
			const double maxOccupancy = 0.9;
			const double efficiencyPercent = 1.0;
			var serviceLevel = new ServiceLevel(new Percent(serviceLevelPercent/100.0), serviceLevelSecond);
			var serviceAgreement = new ServiceAgreement(serviceLevel, new Percent(minOccupancy), new Percent(maxOccupancy));
		
			var shrinkage = new Percent(shrinkagePercent/100.0);
			var efficiency = new Percent(efficiencyPercent);

			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone);
			startDateTime = startDateTime.Add(skill.MidnightBreakOffset);

			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.AddDays(1).Date, skill.TimeZone);
			endDateTime = endDateTime.Add(skill.MidnightBreakOffset);
			var timePeriod = new DateTimePeriod(startDateTime, endDateTime);

			var templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement, new SkillPersonData(), timePeriod)
			{
				Shrinkage = shrinkage,
				Efficiency = efficiency
			};

			foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof (DayOfWeek)))
			{
				var skillDayTemplate = (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, dayOfWeek);
				skillDayTemplate.SetSkillDataPeriodCollection(new List<ITemplateSkillDataPeriod>
				{
					(ITemplateSkillDataPeriod)templateSkillDataPeriod.Clone()
				});
				skill.SetTemplateAt((int) dayOfWeek, skillDayTemplate);
			}
		}

		private void SetOpenHours(IWorkload newWorkload, IEnumerable<OpenHoursInput> openHours)
		{
			if (openHours == null)
				return;
			foreach (var openHoursInput in openHours)
			{
				foreach (var dayOfWeek in openHoursInput.WeekDaySelections)
				{
					if (dayOfWeek.Checked)
					{
						var period = new TimePeriod(openHoursInput.StartTime, openHoursInput.EndTime);
						var workloadDayTemplate = new WorkloadDayTemplate();
						workloadDayTemplate.Create(dayOfWeek.WeekDay.ToString(), DateTime.UtcNow, newWorkload, new List<TimePeriod> { period });
						newWorkload.SetTemplate(dayOfWeek.WeekDay, workloadDayTemplate);
					}
				}
			}
		}
	}

	public class SkillInput
	{
		public string Name { get; set; }
		public Guid ActivityId { get; set; }
		public string TimezoneId { get; set; }
		public Guid[] Queues { get; set; }
		public double ServiceLevelPercent { get; set; }
		public double ServiceLevelSeconds { get; set; }
		public double Shrinkage { get; set; }
		public OpenHoursInput[] OpenHours { get; set; }
	}

	public class OpenHoursInput
	{
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public WeekDaySelection[] WeekDaySelections { get; set; }
	}


	public class WeekDaySelection
	{
		public DayOfWeek WeekDay { get; set; }
		public bool Checked { get; set; }
	}
}