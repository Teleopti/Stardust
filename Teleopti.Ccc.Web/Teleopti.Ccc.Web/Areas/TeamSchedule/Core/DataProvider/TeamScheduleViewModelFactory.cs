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

		public GroupScheduleViewModel CreateViewModel(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex)
		{
			var people = _searchProvider.SearchPermittedPeople(criteriaDictionary, dateInUserTimeZone, DefinedRaptorApplicationFunctionPaths.ViewSchedules).ToArray();
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

			var list = constructGroupScheduleShiftViewModels(dateInUserTimeZone, people, peopleCanSeeConfidentialAbsencesFor, pageSize, currentPageIndex);

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count()
			};
		}

		public GroupScheduleViewModel CreateViewModelForPeople(Guid[] personIds, DateOnly scheduleDate)
		{
			var people = _personRepository.FindPeople(personIds);
			var peopleCanSeeConfidentialAbsencesFor = people.Where(person => _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, scheduleDate, person))
															.Select(person => person.Id.Value);

			var list = constructGroupScheduleShiftViewModels(scheduleDate, people, peopleCanSeeConfidentialAbsencesFor, people.Count(), 1);

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count()
			};
		}


		private IEnumerable<GroupScheduleShiftViewModel> constructGroupScheduleShiftViewModels(DateOnly dateInUserTimeZone, IEnumerable<IPerson> people,
			IEnumerable<Guid> peopleCanSeeConfidentialAbsencesFor, int pageSize, int currentPageIndex)
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

			var sortedPersonScheduleDays = personScheduleDays.OrderBy(
				personScheduleDay =>
				{
					var person = personScheduleDay.Item1;
					var schedule = personScheduleDay.Item2.SingleOrDefault(s => s.DateOnlyAsPeriod.DateOnly == dateInUserTimeZone);
					var isPublished = schedule != null && isSchedulePublished(schedule.DateOnlyAsPeriod.DateOnly.Date, person);
					var sortValue = TeamScheduleSortingUtil.GetSortedValue(schedule, canSeeUnpublishedSchedules, isPublished);
					return sortValue;
				}).ThenBy(personScheduleDay => personScheduleDay.Item1.Name.LastName) ;
			
			var requestedPersonScheduleDays = pageSize > 0 ? sortedPersonScheduleDays.Skip(pageSize * (currentPageIndex - 1)).Take(pageSize) : sortedPersonScheduleDays;

			var list = new List<GroupScheduleShiftViewModel>();
			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var personScheduleDay in requestedPersonScheduleDays)
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
