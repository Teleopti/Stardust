using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkill)]
	public class SkillController : ApiController
	{
		private readonly IActivityProvider _activityProvider;
		private readonly ISkillRepository _skillRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IQueueSourceRepository _queueSourceRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillTypeProvider _skillTypeProvider;
		private readonly IWorkloadRepository _workloadRepository;

		public SkillController(IActivityProvider activityProvider, ISkillRepository skillRepository, IIntervalLengthFetcher intervalLengthFetcher, ILoggedOnUser loggedOnUser, IQueueSourceRepository queueSourceRepository, IActivityRepository activityRepository, ISkillTypeProvider skillTypeProvider, IWorkloadRepository workloadRepository)
		{
			_activityProvider = activityProvider;
			_skillRepository = skillRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_loggedOnUser = loggedOnUser;
			_queueSourceRepository = queueSourceRepository;
			_activityRepository = activityRepository;
			_skillTypeProvider = skillTypeProvider;
			_workloadRepository = workloadRepository;
		}

		[UnitOfWork, Route("api/Skill/Activities"), HttpGet]
		public virtual IEnumerable<dynamic> Activities()
		{
			return _activityProvider.GetAll().Select(activity => new {Id = activity.Id.GetValueOrDefault(), activity.Name});
		}

		[UnitOfWork, Route("api/Skill/Timezones"), HttpGet]
		public virtual dynamic Timezones()
		{
			var defaultTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			return
				new
				{
					DefaultTimezone = defaultTimeZone.Id,
					Timezones = TimeZoneInfo.GetSystemTimeZones().Select(x => new {x.Id, Name = x.DisplayName})
				};
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
			var skills = _skillRepository.LoadAll();
			var skill = skills.FirstOrDefault(x => x.Activity.Id == input.ActivityId);
			var intervalLength = skill != null ? skill.DefaultResolution : _intervalLengthFetcher.IntervalLength;
			var newSkill = new Skill(input.Name, "", Color.Azure, intervalLength, _skillTypeProvider.InboundTelephony())
			{
				TimeZone = TimeZoneInfo.FindSystemTimeZoneById(input.TimezoneId),
				Activity = _activityRepository.Load(input.ActivityId)
			};
			var newWorkload = new Workload(newSkill);
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
			return Ok();
		}
	}

	public class SkillInput
	{
		public string Name { get; set; }
		public Guid ActivityId { get; set; }
		public string TimezoneId { get; set; }
		public Guid[] Queues { get; set; }
	}
}