using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
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
		private readonly IPeopleSearchProvider _searchProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, IScheduleProvider scheduleProvider,
			ITeamScheduleProjectionProvider projectionProvider, ISchedulePersonProvider schedulePersonProvider,
			ILoggedOnUser loggedOnUser, ICommonAgentNameProvider commonAgentNameProvider, IPeopleSearchProvider searchProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_schedulePersonProvider = schedulePersonProvider;
			_loggedOnUser = loggedOnUser;
			_commonAgentNameProvider = commonAgentNameProvider;
			_searchProvider = searchProvider;
		}

		public PagingGroupScheduleShiftViewModel CreateViewModel(Guid groupId, DateTime dateInUserTimeZone, int pageSize, int currentPageIndex)
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
			
			var sortedPersonScheduleDays = personScheduleDays.OrderBy(
				personScheduleDay =>
				{
					var sortValue = getSortedValue(new Tuple<IPerson, IScheduleDay>(personScheduleDay.Item1,
						personScheduleDay.Item2.SingleOrDefault(s => s.DateOnlyAsPeriod.DateOnly.Date == dateInUserTimeZone)),
						canSeeUnpublishedSchedules);
					return sortValue;
				}).ThenBy(personScheduleDay =>　personScheduleDay.Item1.Name.LastName).Skip((currentPageIndex-1) * pageSize).Take(pageSize);

			var list = new List<GroupScheduleShiftViewModel>();
			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var personScheduleDay in sortedPersonScheduleDays)
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
					continue;
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
			return new PagingGroupScheduleShiftViewModel()
			{
				GroupSchedule = list,
				TotalPages = people.Count()/pageSize + 1
			};
		}

		private int getSortedValue(Tuple<IPerson, IScheduleDay> personSchedulePair, bool hasPermissionForUnpublishedSchedule)
		{
			var person = personSchedulePair.Item1;
			var schedule = personSchedulePair.Item2;
			if (schedule == null || !schedule.IsScheduled())
			{
				return 20000;
			}
			var significantPart = schedule.SignificantPart();
			var isPublished = isSchedulePublished(schedule.DateOnlyAsPeriod.DateOnly.Date, person);
			if ((!isPublished && !hasPermissionForUnpublishedSchedule) || (schedule.PersonAssignment() == null && significantPart != SchedulePartView.FullDayAbsence))
			{
				return 20000;
			}

			if (schedule.HasDayOff() || significantPart == SchedulePartView.ContractDayOff)
			{
				return 10000;
			}
			
			if (!schedule.HasDayOff() && significantPart == SchedulePartView.FullDayAbsence)
			{
				var mininumAbsenceStartTime =
					schedule.PersonAbsenceCollection().Select(personAbsence => personAbsence.Period.StartDateTime).Min();

				return 5000 + (int)mininumAbsenceStartTime.Subtract(schedule.DateOnlyAsPeriod.DateOnly.Date).TotalMinutes;
			}
			if (schedule.PersonAssignment() != null)
			{
				return (int)schedule.PersonAssignment().Period.StartDateTime.Subtract(schedule.DateOnlyAsPeriod.DateOnly.Date).TotalMinutes;
			}

			return 0;
		}

		public GroupScheduleViewModel CreateViewModel(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone)
		{
			var people = _searchProvider.SearchPermittedPeople(criteriaDictionary, dateInUserTimeZone, DefinedRaptorApplicationFunctionPaths.TeamSchedule).ToArray();
			if (people.Count() > 500)
			{
				return new GroupScheduleViewModel
				{
					Schedules = new List<GroupScheduleShiftViewModel>(),
					Total = people.Count(),
				}; 
			}
			
			var peopleCanSeeConfidentialAbsencesFor =
				_searchProvider.GetPermittedPersonIdList(criteriaDictionary,9999,1,dateInUserTimeZone,null,DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToList();
			var list = constructGroupScheduleShiftViewModels(dateInUserTimeZone, people, peopleCanSeeConfidentialAbsencesFor);
			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count()
			};
		}

		public GroupScheduleViewModel CreateViewModel(GroupScheduleInput input)
		{
			var people = _schedulePersonProvider.GetPermittedPeople(input, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules).ToArray();
			var peopleCanSeeConfidentialAbsencesFor =
				_schedulePersonProvider.GetPermittedPeople(input,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToList();

			var list = constructGroupScheduleShiftViewModels(new DateOnly(input.ScheduleDate), people,
				peopleCanSeeConfidentialAbsencesFor.Select(p => p.Id.GetValueOrDefault()));

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count()
			};
		}

		private IEnumerable<GroupScheduleShiftViewModel> constructGroupScheduleShiftViewModels(DateOnly dateInUserTimeZone, IEnumerable<IPerson> people,
			IEnumerable<Guid> peopleCanSeeConfidentialAbsencesFor)
		{
			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var scheduleDays = _scheduleProvider.GetScheduleForPersons(dateInUserTimeZone, people).ToList();
			var scheduleDaysForPreviousDay =
				_scheduleProvider.GetScheduleForPersons(dateInUserTimeZone.AddDays(-1), people) ??
				new IScheduleDay[] {};
			scheduleDays.AddRange(
				scheduleDaysForPreviousDay.Where(
					scheduleDay =>
						scheduleDay != null && scheduleDay.PersonAssignment() != null &&
						TimeZoneHelper.ConvertFromUtc(scheduleDay.PersonAssignment().Period.EndDateTime, userTimeZone) >
						dateInUserTimeZone.Date));

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
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person.Id.GetValueOrDefault());
				if (!schedules.Any())
				{
					list.Add(new GroupScheduleShiftViewModel
					{
						PersonId = person.Id.GetValueOrDefault().ToString(),
						Name = nameDescriptionSetting.BuildCommonNameDescription(person),
						Date = dateInUserTimeZone.Date.ToFixedDateFormat(),
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
}
