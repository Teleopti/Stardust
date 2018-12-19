using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core.Extensions;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleShiftViewModelProvider _shiftViewModelProvider;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IUserUiCulture _userUiCulture;
		private readonly IScheduleDayProvider _scheduleDayProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider,
			IScheduleProvider scheduleProvider,
			ITeamScheduleShiftViewModelProvider shiftViewModelProvider,
			IPeopleSearchProvider searchProvider,
			IPersonRepository personRepository,
			IUserUiCulture userUiCulture,
			IScheduleDayProvider scheduleDayProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_shiftViewModelProvider = shiftViewModelProvider;
			_searchProvider = searchProvider;
			_personRepository = personRepository;
			_userUiCulture = userUiCulture;
			_scheduleDayProvider = scheduleDayProvider;
		}

		public GroupScheduleViewModel CreateViewModel(SearchDaySchedulesInput input)
		{
			if (input.NoGroupInput)
				return new GroupScheduleViewModel();

			var period = new DateOnlyPeriod(input.DateInUserTimeZone, input.DateInUserTimeZone);
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
			var personIds =  !input.IsDynamic ? _searchProvider.FindPersonIdsInPeriodWithGroup(week, input.GroupIds, input.CriteriaDictionary)
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
				_scheduleDayProvider.GetScheduleDictionary(scheduleDate, permittedPeople, new ScheduleDictionaryLoadOptions(false, true));

			var list = new List<GroupScheduleShiftViewModel>();
			var dates = new[] { scheduleDate, scheduleDate.AddDays(-1), scheduleDate.AddDays(1) };
			foreach (var person in people)
			{
				var personId = person.Id.GetValueOrDefault();
				var personScheduleRange = schedulesDictionary[person];
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(personId);
				var canSeeUnpublishedSchedules = peopleCanViewUnpublishedFor.Contains(personId);

				list.AddRange(from date in dates
							  let scheduleDay = personScheduleRange.ScheduledDay(date)
							  where scheduleDay != null
							  select _shiftViewModelProvider.MakeViewModel(person, date, scheduleDay, personScheduleRange.ScheduledDay(date.AddDays(-1)), canViewConfidential,
								  canSeeUnpublishedSchedules));
			}

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count
			};
		}

		private Tuple<IPerson, IScheduleDay>[] sortSchedules(Tuple<IPerson,
			IScheduleDay>[] personScheduleDaysToSort,
			TeamScheduleSortOption sortOption,
			Guid[] peopleCanViewUnpublishedFor)
		{
			var stringComparer = StringComparer.Create(_userUiCulture.GetUiCulture(), false);
			switch (sortOption)
			{
				case TeamScheduleSortOption.FirstName:
					personScheduleDaysToSort = personScheduleDaysToSort.OrderBy(ps => ps.Item1.Name.FirstName, stringComparer).ToArray();
					break;
				case TeamScheduleSortOption.LastName:
					personScheduleDaysToSort = personScheduleDaysToSort.OrderBy(ps => ps.Item1.Name.LastName, stringComparer).ToArray();
					break;
				case TeamScheduleSortOption.EmploymentNumber:
					personScheduleDaysToSort = personScheduleDaysToSort.OrderBy(ps => ps.Item1.EmploymentNumber, stringComparer).ToArray();
					break;
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
			return personScheduleDaysToSort;
		}

		private GroupScheduleViewModel createViewModelForPeople(IList<Guid> targetIds, SearchDaySchedulesInput input)
		{
			var date = input.DateInUserTimeZone;

			var permittedPersons = new List<IPerson>();
			var scheduleDays = new List<IScheduleDay>();
			var peopleCanViewUnpublishedFor = new List<Guid>();

			foreach (var batch in targetIds.Batch(251))
			{
				var batchPersons = _personRepository.FindPeople(batch);
				var batchPermittedPersons = _searchProvider
					.GetPermittedPersonList(batchPersons, date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

				if (input.IsOnlyAbsences)
				{
					var batchCanViewUnpublishedFor = _searchProvider
						.GetPermittedPersonIdList(batchPermittedPersons, date, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

					peopleCanViewUnpublishedFor.AddRange(batchCanViewUnpublishedFor);

					var absenceScheduleDaysForPerson = _scheduleProvider.GetScheduleForPersons(date, batchPermittedPersons)
						.Where(sd => (sd.IsFullyPublished || batchCanViewUnpublishedFor.Contains(sd.Person.Id.GetValueOrDefault()))
						&& sd.HasAbsenceProjection());

					scheduleDays.AddRange(absenceScheduleDaysForPerson);

					batchPermittedPersons = absenceScheduleDaysForPerson
						.Select(sd => sd.Person)
						.ToList();
				}
				permittedPersons.AddRange(batchPermittedPersons);
				if (isResultTooMany(permittedPersons))
				{
					return new GroupScheduleViewModel
					{
						Total = targetIds.Count
					};
				}
			}

			if (!permittedPersons.Any()) return new GroupScheduleViewModel();

			if (!input.IsOnlyAbsences)
			{
				scheduleDays = _scheduleProvider.GetScheduleForPersons(date, permittedPersons, true).ToList();
				peopleCanViewUnpublishedFor = _searchProvider
				.GetPermittedPersonIdList(permittedPersons, date, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			}

			var scheduleDayLookup = scheduleDays.ToLookup(s => s.Person);

			var personScheduleDaysToSort = permittedPersons
				.Select(p => new Tuple<IPerson, IScheduleDay>(p, scheduleDayLookup[p].FirstOrDefault())).ToArray();
			personScheduleDaysToSort = sortSchedules(personScheduleDaysToSort, input.SortOption, peopleCanViewUnpublishedFor.ToArray());

			var personScheduleDayPairsForCurrentPage = input.PageSize > 0
				? personScheduleDaysToSort.Skip(input.PageSize * (input.CurrentPageIndex - 1)).Take(input.PageSize).ToList()
				: personScheduleDaysToSort.ToList();

			var personsForCurrentPage = personScheduleDayPairsForCurrentPage.Select(pair => pair.Item1).ToList();

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider.GetPermittedPersonIdList(personsForCurrentPage, date,
				DefinedRaptorApplicationFunctionPaths.ViewConfidential);

			var previousDay = date.AddDays(-1);
			var scheduleDaysForPreviousDayLookup = _scheduleProvider.GetScheduleForPersons(previousDay, personsForCurrentPage).ToLookup(p => p.Person);

			var nextDay = date.AddDays(1);
			var scheduleDaysForNextDayLookup = _scheduleProvider.GetScheduleForPersons(nextDay, personsForCurrentPage).ToLookup(p => p.Person);

			var list = new List<GroupScheduleShiftViewModel>();

			foreach (var pair in personScheduleDayPairsForCurrentPage)
			{
				var person = pair.Item1;
				var personId = person.Id.GetValueOrDefault();
				var currentScheduleDay = pair.Item2;
				var canSeeUnpublishedSchedules = peopleCanViewUnpublishedFor.Contains(personId);
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(personId);
				var schedulePreviousDay = scheduleDaysForPreviousDayLookup.Contains(person) ? scheduleDaysForPreviousDayLookup[person].First() : null;
				var scheduleNextDay = scheduleDaysForNextDayLookup.Contains(person) ? scheduleDaysForNextDayLookup[person].First() : null;

				if (currentScheduleDay != null)
					list.Add(_shiftViewModelProvider.MakeViewModel(person, date, currentScheduleDay, schedulePreviousDay, canViewConfidential, canSeeUnpublishedSchedules));

				if (schedulePreviousDay != null)
					list.Add(_shiftViewModelProvider.MakeViewModel(person, previousDay, schedulePreviousDay, null, canViewConfidential, canSeeUnpublishedSchedules));

				if (scheduleNextDay != null)
					list.Add(_shiftViewModelProvider.MakeViewModel(person, nextDay, scheduleNextDay, currentScheduleDay, canViewConfidential, canSeeUnpublishedSchedules));

			}

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = permittedPersons.Count
			};
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
				var batchPermittedPersons = weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonList(batchedPeople, input.DateInUserTimeZone,
					 DefinedRaptorApplicationFunctionPaths.MyTeamSchedules));

				batchPermittedPersons.ForEach(pg =>
				{
					if (pg.Value.Any())
					{
						permittedPeopleByDate[pg.Key].AddRange(pg.Value);
						pg.Value.ForEach(p => permittedPeopleIds.Add(p.Id.GetValueOrDefault()));
					}
				});

				if (isResultTooMany(permittedPeopleIds))
				{
					return new GroupWeekScheduleViewModel()
					{
						Total = permittedPeopleIds.Count
					};
				}
			}

			if (!permittedPeopleByDate.All(pd => pd.Value.Any()))
			{
				return new GroupWeekScheduleViewModel();
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

			var peopleCanSeeSchedulesFor = permittedPeopleByDate.ToDictionary(pg => pg.Key, pg => pagedPeople.Where(p => pg.Value.Any(pp => pp.Id == p.Id)).Select(p => p.Id.GetValueOrDefault()).ToList());

			var peopleCanSeeUnpublishedSchedulesFor =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, input.DateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));

			var peopleCanSeeConfidentialFor =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, input.DateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential));

			return new GroupWeekScheduleViewModel
			{
				PersonWeekSchedules = pagedPeople
				.Select(person => _shiftViewModelProvider.MakeWeekViewModel(person, weekDays, scheduleDic[person], peopleCanSeeSchedulesFor, peopleCanSeeUnpublishedSchedulesFor, peopleCanSeeConfidentialFor))
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
