using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleViewModelFactory: ITeamScheduleViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ISchedulePersonProvider _schedulePersonProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, IScheduleProvider scheduleProvider, ITeamScheduleProjectionProvider projectionProvider, ISchedulePersonProvider schedulePersonProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_schedulePersonProvider = schedulePersonProvider;
		}
		

		public IEnumerable<GroupScheduleShiftViewModel> CreateViewModel(Guid groupId, DateTime dateInUserTimeZone)
		{
			var people = _schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
				DefinedRaptorApplicationFunctionPaths.MyTeamSchedules).ToArray();
			var peopleCanSeeConfidentialAbsencesFor =
				_schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			var scheduleDays = _scheduleProvider.GetScheduleForPersons(new DateOnly(dateInUserTimeZone), people) ??
							   new IScheduleDay[] { };

			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var personScheduleDays = (from p in people
			 let scheduleDay = (from s in scheduleDays where s.Person == p select s).SingleOrDefault()
			 select new Tuple<IPerson, IScheduleDay>(p, scheduleDay)).ToArray();

			return (from personScheduleDay in personScheduleDays
				let person = personScheduleDay.Item1
				let scheduleDay = personScheduleDay.Item2
				let canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person)
				let isPublished = isSchedulePublished(dateInUserTimeZone, person)
				select scheduleDay != null && (isPublished || canSeeUnpublishedSchedules) ? _projectionProvider.Projection(scheduleDay, canViewConfidential) : new GroupScheduleShiftViewModel
				{
					PersonId = person.Id.GetValueOrDefault().ToString(), Name = person.Name.ToString(), Date = dateInUserTimeZone.ToFixedDateFormat(), Projection = new List<GroupScheduleLayerViewModel>()
				}).ToList();
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