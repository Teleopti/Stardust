using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleShiftViewModelFactory _shiftViewModelFactory;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IToggleManager _toggleManager;
		private readonly IUserUiCulture _userUiCulture;
		private readonly IScheduleDayProvider _scheduleDayProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider,
			IScheduleProvider scheduleProvider,
			ITeamScheduleShiftViewModelFactory shiftViewModelFactory,
			IPeopleSearchProvider searchProvider,
			IPersonRepository personRepository,
			IIanaTimeZoneProvider ianaTimeZoneProvider,
			IToggleManager toggleManager,
			IUserUiCulture userUiCulture,
			IScheduleDayProvider scheduleDayProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_shiftViewModelFactory = shiftViewModelFactory;
			_searchProvider = searchProvider;
			_personRepository = personRepository;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_toggleManager = toggleManager;
			_userUiCulture = userUiCulture;
			_scheduleDayProvider = scheduleDayProvider;
		}

		public GroupScheduleViewModel CreateViewModel(SearchDaySchedulesInput input)
		{
			if (input.NoGroupInput)
				return new GroupScheduleViewModel();

			var period = new DateOnlyPeriod(input.DateInUserTimeZone, input.DateInUserTimeZone);
			var personIds = _toggleManager.IsEnabled(Toggles.Wfm_SearchAgentBasedOnCorrectPeriod_44552) ||
							_toggleManager.IsEnabled(Toggles.Wfm_GroupPages_45057)
							? !input.IsDynamic ? _searchProvider.FindPersonIdsInPeriodWithGroup(period, input.GroupIds, input.CriteriaDictionary)
												: _searchProvider.FindPersonIdsInPeriodWithDynamicGroup(period, input.GroupPageId.GetValueOrDefault(), input.DynamicOptionalValues, input.CriteriaDictionary)
							: _searchProvider.FindPersonIds(input.DateInUserTimeZone, input.GroupIds, input.CriteriaDictionary);


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
			var personIds = new List<Guid>();
			if (_toggleManager.IsEnabled(Toggles.Wfm_SearchAgentBasedOnCorrectPeriod_44552) ||
				_toggleManager.IsEnabled(Toggles.Wfm_GroupPages_45057))
			{
				personIds = !input.IsDynamic ? _searchProvider.FindPersonIdsInPeriodWithGroup(week, input.GroupIds, input.CriteriaDictionary)
												: _searchProvider.FindPersonIdsInPeriodWithDynamicGroup(week, input.GroupPageId.GetValueOrDefault(), input.DynamicOptionalValues, input.CriteriaDictionary);
			}
			else
			{
				personIds = week.DayCollection()
					.SelectMany(d => _searchProvider.FindPersonIds(d, input.GroupIds, input.CriteriaDictionary)).Distinct().ToList();
			}
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

			var peopleCanViewUnpublishedFor = _searchProvider
					.GetPermittedPersonIdList(permittedPeople, scheduleDate, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
					.ToList();

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider
					.GetPermittedPersonIdList(permittedPeople, scheduleDate, DefinedRaptorApplicationFunctionPaths.ViewConfidential)
					.ToList();

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
							  select _shiftViewModelFactory.MakeViewModel(person, date, scheduleDay, personScheduleRange.ScheduledDay(date.AddDays(-1)), canViewConfidential,
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
					Array.Sort(personScheduleDaysToSort, new TeamScheduleComparerNew(peopleCanViewUnpublishedFor, _permissionProvider));
					break;
			}
			return personScheduleDaysToSort;
		}

		private List<IPerson> getPermittedPersons(Guid[] targetIds, DateOnlyPeriod period)
		{
			var matchedPersons = _personRepository.FindPeople(targetIds);
			return _searchProvider
				.GetPermittedPersonList(matchedPersons, period, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)
				.ToList();
		}
		private GroupScheduleViewModel createViewModelForPeople(IList<Guid> targetIds, SearchDaySchedulesInput input)
		{
			var date = input.DateInUserTimeZone;

			var permittedPersons = new List<IPerson>();
			var scheduleDays = new List<IScheduleDay>();
			var peopleCanViewUnpublishedFor = new List<Guid>();

			foreach (var batch in targetIds.Batch(251))
			{
				var batchPersons = _personRepository.FindPeople(targetIds);
				var batchPermittedPersons = _searchProvider
					.GetPermittedPersonList(batchPersons, date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)
				   .ToList();

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
						Schedules = new List<GroupScheduleShiftViewModel>(),
						Total = targetIds.Count
					};
				}
			}

			if (!input.IsOnlyAbsences)
			{
				scheduleDays = _scheduleProvider.GetScheduleForPersons(date, permittedPersons, true).ToList();
			}

			var scheduleDayLookup = scheduleDays.ToLookup(s => s.Person);

			var personScheduleDaysToSort = permittedPersons
				.Select(p => new Tuple<IPerson, IScheduleDay>(p, scheduleDayLookup.Contains(p) ? scheduleDayLookup[p].FirstOrDefault() : null)).ToArray();
			personScheduleDaysToSort = sortSchedules(personScheduleDaysToSort, input.SortOption, peopleCanViewUnpublishedFor.ToArray());

			var personScheduleDayPairsForCurrentPage = input.PageSize > 0
				? personScheduleDaysToSort.Skip(input.PageSize * (input.CurrentPageIndex - 1)).Take(input.PageSize).ToList()
				: personScheduleDaysToSort.ToList();

			var personsForCurrentPage = personScheduleDayPairsForCurrentPage.Select(pair => pair.Item1).ToList();

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider.GetPermittedPersonIdList(personsForCurrentPage, date,
				DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToList();
			if (!input.IsOnlyAbsences)
			{
				peopleCanViewUnpublishedFor = _searchProvider
					.GetPermittedPersonIdList(personsForCurrentPage, date, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
				   .ToList();
			}
			else
			{
				peopleCanViewUnpublishedFor = peopleCanViewUnpublishedFor.Where(pid => personsForCurrentPage.Any(p => p.Id == pid)).ToList();
			}

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
					list.Add(_shiftViewModelFactory.MakeViewModel(person, date, currentScheduleDay, schedulePreviousDay, canViewConfidential, canSeeUnpublishedSchedules));

				if (schedulePreviousDay != null)
					list.Add(_shiftViewModelFactory.MakeViewModel(person, previousDay, schedulePreviousDay, null, canViewConfidential, canSeeUnpublishedSchedules));

				if (scheduleNextDay != null)
					list.Add(_shiftViewModelFactory.MakeViewModel(person, nextDay, scheduleNextDay, currentScheduleDay, canViewConfidential, canSeeUnpublishedSchedules));

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
			var people = new List<IPerson>();
			foreach (var batch in personIds.Batch(251))
			{
				var batchPermittedPersons = getPermittedPersons(batch.ToArray(), week);
				people.AddRange(batchPermittedPersons);
				if (isResultTooMany(people))
				{
					return new GroupWeekScheduleViewModel
					{
						PersonWeekSchedules = new List<PersonWeekScheduleViewModel>(),
						Total = people.Count
					};
				}
			}

			var pagedPeople = input.PageSize > 0
				? people.OrderBy(p => p.Name.LastName).Skip(input.PageSize * (input.CurrentPageIndex - 1)).Take(input.PageSize).ToList()
				: people.ToList();

			var scheduleDays = _scheduleProvider.GetScheduleForPersonsInPeriod(week.Inflate(1), pagedPeople)
				.ToDictionary(sd => new PersonDate
				{
					PersonId = sd.Person.Id.GetValueOrDefault(),
					Date = sd.DateOnlyAsPeriod.DateOnly
				});

			var peopleCanSeeUnpublishedSchedulesFor =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, input.DateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));

			var peopleCanSeeConfidentialFor =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, input.DateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential));

			return new GroupWeekScheduleViewModel
			{
				PersonWeekSchedules = pagedPeople
				.Select(person => _shiftViewModelFactory.MakeWeekViewModel(person, weekDays, scheduleDays, peopleCanSeeUnpublishedSchedulesFor, peopleCanSeeConfidentialFor))
				.ToList(),
				Total = people.Count
			};
		}



		private bool isResultTooMany(IList<IPerson> people)
		{
			var max = _toggleManager.IsEnabled(Toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871) ? 750 : 500;
			return people.Count > max;

		}
	}
}
