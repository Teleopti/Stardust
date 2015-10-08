using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
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

		public SkillController(IActivityProvider activityProvider, ISkillRepository skillRepository, IIntervalLengthFetcher intervalLengthFetcher, ILoggedOnUser loggedOnUser)
		{
			_activityProvider = activityProvider;
			_skillRepository = skillRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork, Route("api/Skill/Activities"), HttpGet]
		public virtual IEnumerable<dynamic> Activities()
		{
			var skills = _skillRepository.LoadAll();
			var activities =_activityProvider.GetAll();
			var result = new List<dynamic>();
			foreach (var activity in activities)
			{
				var skill = skills.FirstOrDefault(x => x.Activity.Id == activity.Id);
				var intervalLength = skill != null ? skill.DefaultResolution : _intervalLengthFetcher.IntervalLength;
				result.Add(new { Id = activity.Id.GetValueOrDefault(), activity.Name, IntervalLength = intervalLength });
			}
			return result;
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
	}
}