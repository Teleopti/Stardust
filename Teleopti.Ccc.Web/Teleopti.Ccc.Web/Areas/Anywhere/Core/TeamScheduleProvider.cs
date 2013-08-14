﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleProvider : ITeamScheduleProvider
	{
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ISchedulePersonProvider _schedulePersonProvider;

		public TeamScheduleProvider(IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IPermissionProvider permissionProvider, ISchedulePersonProvider schedulePersonProvider)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_permissionProvider = permissionProvider;
			_schedulePersonProvider = schedulePersonProvider;
		}

		public IEnumerable<PersonScheduleDayReadModel> TeamSchedule(Guid teamId, DateTime date)
		{
			date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));

			var schedule = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId);
			if (schedule != null)
			{
				return
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
						? schedule
						: filterPublishedScheduleForTeamAndDate(teamId, date, schedule);
			}
			return new List<PersonScheduleDayReadModel>();
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