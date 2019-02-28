using Microsoft.ReportingServices.RdlExpressions.ExpressionHostObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core.Extensions;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly TeamScheduleShiftViewModelProvider _shiftViewModelProvider;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IUserUiCulture _userUiCulture;
		private readonly IScheduleDayProvider _scheduleDayProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider,
			IScheduleProvider scheduleProvider,
			TeamScheduleShiftViewModelProvider shiftViewModelProvider,
			IPeopleSearchProvider searchProvider,
			IPersonRepository personRepository,
			IUserUiCulture userUiCulture,
			IScheduleDayProvider scheduleDayProvider,
			ICommonAgentNameProvider commonAgentNameProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_shiftViewModelProvider = shiftViewModelProvider;
			_searchProvider = searchProvider;
			_personRepository = personRepository;
			_userUiCulture = userUiCulture;
			_scheduleDayProvider = scheduleDayProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
		}

		public GroupScheduleViewModel CreateViewModel(SearchDaySchedulesInput input)
		{
			if (input.NoGroupInput)
				return new GroupScheduleViewModel();

			var period = input.DateInUserTimeZone.ToDateOnlyPeriod();
			var personIds = !input.IsDynamic ? _searchProvider.FindPersonIdsInPeriodWithGroup(period, input.GroupIds, input.CriteriaDictionary)
												: _searchProvider.FindPersonIdsInPeriodWithDynamicGroup(period, input.GroupPageId.GetValueOrDefault(), input.DynamicOptionalValues, input.CriteriaDictionary);

			return createViewModelForPeople(personIds, input);
		}

		public GroupWeekScheduleViewModel CreateWeekScheduleViewModel(SearchSchedulesInput input)
		{
			if (input.NoGroupInput)
				return new GroupWeekScheduleViewModel
				{
					PersonWeekSchedules = new List<PersonWeekScheduleViewModel>(),
					Total = 0,
					Keyword = ""
				};

			var week = DateHelper.GetWeekPeriod(input.DateInUserTimeZone, DateTimeFormatExtensions.FirstDayOfWeek);
			var personIds = !input.IsDynamic ? _searchProvider.FindPersonIdsInPeriodWithGroup(week, input.GroupIds, input.CriteriaDictionary)
												: _searchProvider.FindPersonIdsInPeriodWithDynamicGroup(week, input.GroupPageId.GetValueOrDefault(), input.DynamicOptionalValues, input.CriteriaDictionary);

			return createWeekViewModelForPeople(personIds, input);
		}

		public GroupScheduleViewModel CreateViewModelForPeople(Guid[] personIds, DateOnly scheduleDate)
		{
			if (personIds.IsNullOrEmpty() || scheduleDate.Date.Date == DateTime.MinValue)
			{
				return new GroupScheduleViewModel();
			}
			var people = _personRepository.FindPeople(personIds);

			var permittedPeople = _searchProvider.GetPermittedPersonList(people, scheduleDate, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			if (!permittedPeople.Any()) return new GroupScheduleViewModel();

			var peopleCanViewUnpublishedFor = _searchProvider
					.GetPermittedPersonIdList(permittedPeople, scheduleDate, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider
					.GetPermittedPersonIdList(permittedPeople, scheduleDate, DefinedRaptorApplicationFunctionPaths.ViewConfidential);

			var schedulesDictionary =
				_scheduleDayProvider.GetScheduleDictionary(scheduleDate.ToDateOnlyPeriod().Inflate(1), permittedPeople, new ScheduleDictionaryLoadOptions(false, true));

			var list = new List<GroupScheduleShiftViewModel>();
			var dates = new[] { scheduleDate, scheduleDate.AddDays(-1), scheduleDate.AddDays(1) };

			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var person in people)
			{
				var personId = person.Id.GetValueOrDefault();
				var personScheduleRange = schedulesDictionary[person];
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(personId);
				var canSeeUnpublishedSchedules = peopleCanViewUnpublishedFor.Contains(personId);

				list.AddRange(from date in dates
							  let scheduleDay = personScheduleRange.ScheduledDay(date)
							  where scheduleDay != null
							  select _shiftViewModelProvider.MakeViewModel(person, date, scheduleDay,
								  personScheduleRange.ScheduledDay(date.AddDays(-1)),
								  commonAgentNameSettings,
								  canViewConfidential,
									canSeeUnpublishedSchedules,
									date == scheduleDate));
			}

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count
			};
		}

		private GroupScheduleViewModel createViewModelForPeople(IList<Guid> targetIds, SearchDaySchedulesInput input)
		{
			var date = input.DateInUserTimeZone;
			var schedulePeriod = date.ToDateOnlyPeriod().Inflate(1);
			var permittedPeople = new List<IPerson>();
			var scheduleDays = new List<IScheduleDay>();
			var peopleCanViewUnpublishedFor = new List<Guid>();

			foreach (var batch in targetIds.Batch(251))
			{
				addPermittedPeopleAndLoadSchedulesIfLoadOnlyAbsences(date,
					schedulePeriod,
					batch,
					permittedPeople,
					input.IsOnlyAbsences,
					peopleCanViewUnpublishedFor,
					scheduleDays);

				if (isResultTooMany(permittedPeople))
				{
					return new GroupScheduleViewModel
					{
						Total = targetIds.Count
					};
				}
			}

			if (!permittedPeople.Any()) return new GroupScheduleViewModel();

			var stringComparer = StringComparer.Create(_userUiCulture.GetUiCulture(), false);
			var (sortedPermittedPeople, isSortedByPersonInfo) = sortByPersonInfo(permittedPeople, input);
			var permittedPersonCount = permittedPeople.Count();

			IList<Tuple<IPerson, IScheduleDay>> personScheduleDayPairsForCurrentPage = null;
			List<IPerson> personsForCurrentPage = null;

			if (!input.IsOnlyAbsences)
			{
				scheduleDays = _scheduleProvider.GetScheduleForPersonsInPeriod(schedulePeriod, sortedPermittedPeople, new ScheduleDictionaryLoadOptions(false, true)).ToList();
				peopleCanViewUnpublishedFor = _searchProvider.GetPermittedPersonIdList(permittedPeople, date, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules).ToList();
			}

			var scheduleDayLookup = scheduleDays.ToLookup(s => (s.Person, s.DateOnlyAsPeriod.DateOnly));
			if (isSortedByPersonInfo)
			{
				personsForCurrentPage = sortedPermittedPeople;
				personScheduleDayPairsForCurrentPage = sortedPermittedPeople
				.Select(p => new Tuple<IPerson, IScheduleDay>(p, scheduleDayLookup[(p, date)].FirstOrDefault())).ToList();
			}
			else
			{
				var personScheduleDaysToSort = sortedPermittedPeople.Select(p => new Tuple<IPerson, IScheduleDay>(p, scheduleDayLookup[(p, date)].FirstOrDefault())).ToArray();
				sortBySchedules(personScheduleDaysToSort, input.SortOption, peopleCanViewUnpublishedFor.ToArray());
				personScheduleDayPairsForCurrentPage = pageItems(personScheduleDaysToSort, input);
				personsForCurrentPage = personScheduleDayPairsForCurrentPage.Select(pair => pair.Item1).ToList();
			}

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider.GetPermittedPersonIdList(personsForCurrentPage, date, DefinedRaptorApplicationFunctionPaths.ViewConfidential);

			var previousDay = date.AddDays(-1);
			var nextDay = date.AddDays(1);

			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			var list = new List<GroupScheduleShiftViewModel>();

			foreach (var pair in personScheduleDayPairsForCurrentPage)
			{
				var person = pair.Item1;
				var personId = person.Id.GetValueOrDefault();
				var currentScheduleDay = pair.Item2;
				var canSeeUnpublishedSchedules = peopleCanViewUnpublishedFor.Contains(personId);
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(personId);
				var schedulePreviousDay = scheduleDayLookup[(person, previousDay)].FirstOrDefault();
				var scheduleNextDay = scheduleDayLookup[(person, nextDay)].FirstOrDefault();

				if (currentScheduleDay != null)
					list.Add(_shiftViewModelProvider.MakeViewModel(person, date, currentScheduleDay, schedulePreviousDay, commonAgentNameSettings, canViewConfidential, canSeeUnpublishedSchedules, true));

				if (schedulePreviousDay != null)
					list.Add(_shiftViewModelProvider.MakeViewModel(person, previousDay, schedulePreviousDay, null, commonAgentNameSettings, canViewConfidential, canSeeUnpublishedSchedules));

				if (scheduleNextDay != null)
					list.Add(_shiftViewModelProvider.MakeViewModel(person, nextDay, scheduleNextDay, currentScheduleDay, commonAgentNameSettings, canViewConfidential, canSeeUnpublishedSchedules));

			}

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = permittedPersonCount
			};
		}

		private void addPermittedPeopleAndLoadSchedulesIfLoadOnlyAbsences(
			DateOnly date,
			DateOnlyPeriod schedulePeriod,
			IEnumerable<Guid> personIds,
			List<IPerson> permittedPeople,
			bool onlyAbsences,
			List<Guid> peopleCanViewUnpublishedFor,
			List<IScheduleDay> scheduleDays)
		{
			var batchPeople = _personRepository.FindPeople(personIds);
			var batchPermittedPeople = _searchProvider
				.GetPermittedPersonList(batchPeople, date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			if (onlyAbsences)
			{
				var batchCanViewUnpublishedFor = _searchProvider
					.GetPermittedPersonIdList(batchPermittedPeople, date, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

				peopleCanViewUnpublishedFor.AddRange(batchCanViewUnpublishedFor);

				var absenceScheduleDaysForPerson = _scheduleProvider.GetScheduleForPersonsInPeriod(schedulePeriod, batchPermittedPeople)
					.Where(sd => (sd.IsFullyPublished || batchCanViewUnpublishedFor.Contains(sd.Person.Id.GetValueOrDefault()))
					&& sd.HasAbsenceProjection())
					.ToList();

				scheduleDays.AddRange(absenceScheduleDaysForPerson);

				batchPermittedPeople = absenceScheduleDaysForPerson
					.Select(sd => sd.Person)
					.Distinct()
					.ToList();
			}
			permittedPeople.AddRange(batchPermittedPeople);
		}

		private (List<IPerson>, bool) sortByPersonInfo(List<IPerson> people, SearchDaySchedulesInput input)
		{
			var stringComparer = StringComparer.Create(_userUiCulture.GetUiCulture(), false);
			switch (input.SortOption)
			{
				case TeamScheduleSortOption.FirstName:
					return (pageItems(people.OrderBy(p => p.Name.FirstName, stringComparer), input), true);
				case TeamScheduleSortOption.LastName:
					return (pageItems(people.OrderBy(p => p.Name.LastName, stringComparer), input), true);
				case TeamScheduleSortOption.EmploymentNumber:
					return (pageItems(people.OrderBy(p => p.EmploymentNumber, stringComparer), input), true);
			}
			return (people, false);
		}

		private void sortBySchedules(Tuple<IPerson,
				IScheduleDay>[] personScheduleDaysToSort,
				TeamScheduleSortOption sortOption,
				Guid[] peopleCanViewUnpublishedFor)
		{
			var stringComparer = StringComparer.Create(_userUiCulture.GetUiCulture(), false);
			switch (sortOption)
			{
				case TeamScheduleSortOption.StartTime:
					Array.Sort(personScheduleDaysToSort, new TeamScheduleTimeComparer(period => period.StartDateTime, _permissionProvider, stringComparer));
					break;
				case TeamScheduleSortOption.EndTime:
					Array.Sort(personScheduleDaysToSort, new TeamScheduleTimeComparer(period => period.EndDateTime, _permissionProvider, stringComparer));
					break;
				default:
					Array.Sort(personScheduleDaysToSort, new TeamScheduleComparerNew(peopleCanViewUnpublishedFor));
					break;
			}
		}
		private List<T> pageItems<T>(IEnumerable<T> items, SearchDaySchedulesInput input)
		{
			return input.PageSize > 0
				? items.Skip(input.PageSize * (input.CurrentPageIndex - 1)).Take(input.PageSize).ToList()
				: items.ToList();
		}

		private GroupWeekScheduleViewModel createWeekViewModelForPeople(IList<Guid> personIds, SearchSchedulesInput input)
		{
			var week = DateHelper.GetWeekPeriod(input.DateInUserTimeZone, DateTimeFormatExtensions.FirstDayOfWeek);
			var weekDays = week.DayCollection();
			var permittedPeopleByDate = weekDays.ToDictionary(d => d, d => new List<IPerson>());
			var permittedPeopleIds = new HashSet<Guid>();

			foreach (var batch in personIds.Batch(251))
			{
				var batchedPeople = _personRepository.FindPeople(batch);
				var batchPermittedPeople = weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonList(batchedPeople, d,
					 DefinedRaptorApplicationFunctionPaths.MyTeamSchedules));

				batchPermittedPeople.ForEach(pg =>
				{
					if (pg.Value.Any())
					{
						permittedPeopleByDate[pg.Key].AddRange(pg.Value);
						pg.Value.ForEach(p => permittedPeopleIds.Add(p.Id.GetValueOrDefault()));
					}
				});

				if (isResultTooMany(permittedPeopleIds))
				{
					return new GroupWeekScheduleViewModel
					{
						Total = permittedPeopleIds.Count
					};
				}
			}

			var allPermittedPeople = permittedPeopleByDate
					.SelectMany(pg => pg.Value)
					.ToLookup(p => p.Id)
					.Select(p => p.First())
					.ToList();

			var pagedPeople = input.PageSize > 0
				? allPermittedPeople
					.OrderBy(p => p.Name.LastName)
					.Skip(input.PageSize * (input.CurrentPageIndex - 1))
					.Take(input.PageSize).ToList()
				: allPermittedPeople;

			var scheduleDic = _scheduleDayProvider.GetScheduleDictionary(week, pagedPeople);

			var peopleCanSeeSchedulesFor = permittedPeopleByDate.ToDictionary(pg => pg.Key, pg => pagedPeople.Where(p => pg.Value.Any(pp => pp.Id == p.Id)).Select(p => p.Id.GetValueOrDefault()).ToHashSet());

			var peopleCanSeeUnpublishedSchedulesFor =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, d,
					DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));

			var peopleCanSeeConfidentialFor =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, d,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential));

			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			return new GroupWeekScheduleViewModel
			{
				PersonWeekSchedules = pagedPeople
				.Select(person => _shiftViewModelProvider.MakeWeekViewModel(person,
																			weekDays,
																			scheduleDic[person],
																			peopleCanSeeSchedulesFor,
																			peopleCanSeeUnpublishedSchedulesFor,
																			peopleCanSeeConfidentialFor,
																			nameDescriptionSetting))
				.ToList(),
				Total = permittedPeopleIds.Count
			};
		}

		private bool isResultTooMany<T>(IEnumerable<T> people)
		{
			return people.Count() > 500;
		}
	}
}
