using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ISchedulePersonProvider _schedulePersonProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, IScheduleProvider scheduleProvider,
			ITeamScheduleProjectionProvider projectionProvider, ISchedulePersonProvider schedulePersonProvider,
			ILoggedOnUser loggedOnUser, ICommonAgentNameProvider commonAgentNameProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_schedulePersonProvider = schedulePersonProvider;
			_loggedOnUser = loggedOnUser;
			_commonAgentNameProvider = commonAgentNameProvider;
		}

		public IEnumerable<GroupScheduleShiftViewModel> CreateViewModel(Guid groupId, DateTime dateInUserTimeZone)
		{
			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var people = _schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
				DefinedRaptorApplicationFunctionPaths.MyTeamSchedules).ToArray();
			var peopleCanSeeConfidentialAbsencesFor =
				_schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToList();
			var scheduleDays = _scheduleProvider.GetScheduleForPersons(new DateOnly(dateInUserTimeZone), people).ToList();
			var scheduleDaysForPreviousDay =
				_scheduleProvider.GetScheduleForPersons(new DateOnly(dateInUserTimeZone).AddDays(-1), people) ??
				new IScheduleDay[] {};
			scheduleDays.AddRange(
				scheduleDaysForPreviousDay.Where(
					scheduleDay =>
						scheduleDay != null && scheduleDay.PersonAssignment() != null &&
						TimeZoneHelper.ConvertFromUtc(scheduleDay.PersonAssignment().Period.EndDateTime, userTimeZone) >
						dateInUserTimeZone));

			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var personScheduleDays = (from p in people
				let personSchedules = (from s in scheduleDays where s.Person == p select s)
				select new Tuple<IPerson, IEnumerable<IScheduleDay>>(p, personSchedules)).ToArray();

			var list = new List<GroupScheduleShiftViewModel>();
			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var personScheduleDay in personScheduleDays)
			{
				var person = personScheduleDay.Item1;
				var schedules = personScheduleDay.Item2.ToArray();
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person);
				if (!schedules.Any())
				{
					list.Add(new GroupScheduleShiftViewModel
					{
						PersonId = person.Id.GetValueOrDefault().ToString(),
						Name = nameDescriptionSetting.BuildCommonNameDescription(person),
						Date = dateInUserTimeZone.ToFixedDateFormat(),
						Projection = new List<GroupScheduleLayerViewModel>()
					});
				}

				foreach (var scheduleDay in schedules)
				{
					var isPublished = isSchedulePublished(scheduleDay.DateOnlyAsPeriod.DateOnly.Date, person);
					list.Add(isPublished || canSeeUnpublishedSchedules
						? _projectionProvider.Projection(scheduleDay, canViewConfidential)
						: new GroupScheduleShiftViewModel
						{
							PersonId = person.Id.GetValueOrDefault().ToString(),
							Name = nameDescriptionSetting.BuildCommonNameDescription(person),
							Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToFixedDateFormat(),
							Projection = new List<GroupScheduleLayerViewModel>()
						});
				}
			}
			return list;
		}

		private static bool isSchedulePublished(DateTime date, IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return false;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
				   workflowControlSet.SchedulePublishedToDate.Value >= date.Date;
		}
	}

	public interface ITeamScheduleViewModelFactory
	{
		IEnumerable<GroupScheduleShiftViewModel> CreateViewModel(Guid groupId, DateTime dateInUserTimeZone);
	}
}
