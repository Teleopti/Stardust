﻿using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	[PermissionCheck(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, nameof(Resources.YouDoNotHavePermissionsToViewTeamSchedules))]
	[PermissionCheck(DefinedRaptorApplicationFunctionPaths.ViewSchedules, nameof(Resources.NoPermissionToViewSchedules))]
	public class TeamScheduleActivityCommandController : ApiController
	{
		private readonly TeamScheduleActivityCommandService _activityCommandService;

		public TeamScheduleActivityCommandController(TeamScheduleActivityCommandService activityCommandService)
		{
			_activityCommandService = activityCommandService;
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/AddActivity")]
		public virtual List<ActionResult> AddActivityCommand([FromBody]AddActivityFormData input)
		{
			return _activityCommandService.AddActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/AddPersonalActivity")]
		public virtual List<ActionResult> AddPersonalActivityCommand([FromBody]AddPersonalActivityFormData input)
		{
			return _activityCommandService.AddPersonalActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamSchedule/AddOvertimeActivity")]
		public virtual IList<ActionResult> AddOvertimeActivity([FromBody] AddOvertimeActivityForm input)
		{
			return _activityCommandService.AddOvertimeActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/RemoveActivity")]
		public virtual List<ActionResult> RemoveActivityCommand([FromBody]RemoveActivityFormData input)
		{
			return _activityCommandService.RemoveActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/MoveActivity")]
		public virtual List<ActionResult> MoveActivityCommand([FromBody] MoveActivityFormData input)
		{
			return _activityCommandService.MoveActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamSchedule/MoveShift")]
		public virtual IList<ActionResult> MoveShift([FromBody] MoveShiftForm input)
		{
			return _activityCommandService.MoveShift(input);
		}
	}
}