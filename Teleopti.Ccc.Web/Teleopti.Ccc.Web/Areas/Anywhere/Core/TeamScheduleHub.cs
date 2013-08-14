using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{

	[HubName("teamScheduleHub")]
	public class TeamScheduleHub : TestableHub
	{
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ISchedulePersonProvider _schedulePersonProvider;

		public TeamScheduleHub(IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IPermissionProvider permissionProvider, ISchedulePersonProvider schedulePersonProvider)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_permissionProvider = permissionProvider;
			_schedulePersonProvider = schedulePersonProvider;
		}

		[UnitOfWork]
		public void SubscribeTeamSchedule(Guid teamId, DateTime date)
		{
			pushSchedule(Clients.Caller, teamId, date);
		}

		private void pushSchedule(dynamic target, Guid teamId, DateTime date)
		{
			date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));
			var schedule = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId);

			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
			{
				if (schedule != null)
					target.incomingTeamSchedule(schedule.Select(s => JsonConvert.DeserializeObject<ExpandoObject>(s.Shift)));
			}
			else
			{
				var result = filterPublishedScheduleForTeamAndDate(teamId, date, schedule);
				target.incomingTeamSchedule(result.Select(s => JsonConvert.DeserializeObject<ExpandoObject>(s.Shift)));
			}
		}

		private IEnumerable<PersonScheduleDayReadModel> filterPublishedScheduleForTeamAndDate(Guid teamId, DateTime date, IEnumerable<PersonScheduleDayReadModel> schedule)
		{
			var result = new List<PersonScheduleDayReadModel>();
			var persons = _schedulePersonProvider.GetPermittedPersonsForTeam(new DateOnly(date), teamId,
																			 DefinedRaptorApplicationFunctionPaths
																				 .SchedulesAnywhere);
			foreach (var personScheduleDayReadModel in schedule)
			{
				var person = (from p in persons where (p.Id == personScheduleDayReadModel.PersonId) select p).FirstOrDefault();

				if (person != null && IsSchedulePublished(date, person))
					result.Add(personScheduleDayReadModel);
			}
			return result;
		}

		private bool IsSchedulePublished(DateTime date, IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
			       workflowControlSet.SchedulePublishedToDate.Value.AddDays(1) > date;
		}
	}
}