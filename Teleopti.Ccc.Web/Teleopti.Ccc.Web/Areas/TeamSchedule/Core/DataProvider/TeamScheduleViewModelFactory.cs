using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPersonRepository _personRepository;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, IScheduleProvider scheduleProvider,
			ITeamScheduleProjectionProvider projectionProvider, ILoggedOnUser loggedOnUser, ICommonAgentNameProvider commonAgentNameProvider, IPeopleSearchProvider searchProvider, IPersonRepository personRepository)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_loggedOnUser = loggedOnUser;
			_commonAgentNameProvider = commonAgentNameProvider;
			_searchProvider = searchProvider;
			_personRepository = personRepository;
		}

		public GroupScheduleViewModel CreateViewModel(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex, bool isOnlyAbsences)
		{
			IPerson[] people;
			if (isOnlyAbsences)
			{
				people =
					_searchProvider.SearchPermittedPeopleWithAbsence(criteriaDictionary, dateInUserTimeZone,
						DefinedRaptorApplicationFunctionPaths.ViewSchedules).ToArray();
				people = filterAbsenceOutOfSchedule(dateInUserTimeZone, people);
			}
			else
			{
				people = _searchProvider.SearchPermittedPeople(criteriaDictionary, dateInUserTimeZone, DefinedRaptorApplicationFunctionPaths.ViewSchedules).ToArray();
			}

			if (people.Length > 500)
			{
				return new GroupScheduleViewModel
				{
					Schedules = new List<GroupScheduleShiftViewModel>(),
					Total = people.Length,
				}; 
			}
			
			var peopleCanSeeConfidentialAbsencesFor =
				_searchProvider.GetPermittedPersonIdList(criteriaDictionary,9999,1,dateInUserTimeZone,null,DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToArray();

			var list = constructGroupScheduleShiftViewModels(dateInUserTimeZone, people, peopleCanSeeConfidentialAbsencesFor, pageSize, currentPageIndex);

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Length
			};
		}

		public GroupScheduleViewModel CreateViewModelForPeople(Guid[] personIds, DateOnly scheduleDate)
		{
			var people = _personRepository.FindPeople(personIds);
			var peopleCanSeeConfidentialAbsencesFor = people.Where(
				person => _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
					scheduleDate, person))
				.Select(person => person.Id.GetValueOrDefault()).ToArray();

			var list = constructGroupScheduleShiftViewModels(scheduleDate, people, peopleCanSeeConfidentialAbsencesFor,
				people.Count, 1);

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count
			};
		}

		private IPerson[] filterAbsenceOutOfSchedule(DateOnly dateInUserTimeZone, IPerson[] people)
		{
			var scheduleDays = _scheduleProvider.GetScheduleForPersons(dateInUserTimeZone, people).ToList();
			var ret = new List<IPerson>();
			foreach (var scheduleDay in scheduleDays)
			{
				var personAssignment = scheduleDay.PersonAssignment();
				if (personAssignment != null)
				{
					var schedulePeriod = personAssignment.Period;
					var personAbsences = scheduleDay.PersonAbsenceCollection();
					var isAbsenceOutOfSchedule = true;
					foreach (var personAbsence in personAbsences)
					{
						if (personAbsence.Period.Contains(schedulePeriod) || schedulePeriod.Contains(personAbsence.Period)) isAbsenceOutOfSchedule = false;
					}
					if (isAbsenceOutOfSchedule) continue;
					ret.Add(scheduleDay.Person);
				}
			}
			return ret.ToArray();
		}

		private IEnumerable<GroupScheduleShiftViewModel> constructGroupScheduleShiftViewModels(DateOnly dateInUserTimeZone,
			ICollection<IPerson> people, Guid[] peopleCanSeeConfidentialAbsencesFor, int pageSize, int currentPageIndex)
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
			var lookupSchedule = scheduleDays.ToLookup(s => s.Person);
			var personScheduleDaysToSort = people.Select(p =>
			{
				var schedule = lookupSchedule[p].SingleOrDefault(s => s.DateOnlyAsPeriod.DateOnly == dateInUserTimeZone);
				return new Tuple<IPerson, IScheduleDay>(p,schedule);
			}).ToArray();

			Array.Sort(personScheduleDaysToSort, new TeamScheduleComparer(canSeeUnpublishedSchedules,_permissionProvider));

			var requestedPersonScheduleDayPairs = pageSize > 0 ? personScheduleDaysToSort.Skip(pageSize * (currentPageIndex - 1)).Take(pageSize) : personScheduleDaysToSort;
			var requestedPersonScheduleDays = requestedPersonScheduleDayPairs.Select(pair =>
			{
				var person = pair.Item1;
				var schedules = new List<IScheduleDay>();
				if (pair.Item2 != null)
				{
					schedules.Add(pair.Item2);
				}
				schedules.AddRange(lookupSchedule[person].Where(s => s.DateOnlyAsPeriod.DateOnly != dateInUserTimeZone));
				return new {Person = person, Schedules = schedules};
			});

			var list = new List<GroupScheduleShiftViewModel>();
			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var personScheduleDay in requestedPersonScheduleDays)
			{
				var person = personScheduleDay.Person;
				var schedules = personScheduleDay.Schedules.ToArray();
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person.Id.GetValueOrDefault());
				if (!schedules.Any())
				{
					list.Add(new GroupScheduleShiftViewModel
					{
						PersonId = person.Id.GetValueOrDefault().ToString(),
						Name = nameDescriptionSetting.BuildCommonNameDescription(person),
						Date = dateInUserTimeZone.Date.ToFixedDateFormat(),
						Projection = new List<GroupScheduleProjectionViewModel>()
					});
				}

				foreach (var scheduleDay in schedules)
				{
					var isPublished = isSchedulePublished(scheduleDay.DateOnlyAsPeriod.DateOnly, person);
					list.Add(isPublished || canSeeUnpublishedSchedules
						? _projectionProvider.Projection(scheduleDay, canViewConfidential, nameDescriptionSetting)
						: new GroupScheduleShiftViewModel
						{
							PersonId = person.Id.GetValueOrDefault().ToString(),
							Name = nameDescriptionSetting.BuildCommonNameDescription(person),
							Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToFixedDateFormat(),
							Projection = new List<GroupScheduleProjectionViewModel>()
						});
				}
			}
			return list;
		}
		private static bool isSchedulePublished(DateOnly date, IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return false;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
				   workflowControlSet.SchedulePublishedToDate.Value >= date.Date;
		}
	}
}
