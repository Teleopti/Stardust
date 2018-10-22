﻿using System;
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
		private readonly ITeamScheduleProjectionProvider _teamScheduleProjectionProvider;
		private readonly IProjectionProvider _projectionProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IToggleManager _toggleManager;
		private readonly IUserUiCulture _userUiCulture;
		private readonly IScheduleDayProvider _scheduleDayProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, IScheduleProvider scheduleProvider,
			ITeamScheduleProjectionProvider teamScheduleProjectionProvider, IProjectionProvider projectionProvider,
			ICommonAgentNameProvider commonAgentNameProvider, IPeopleSearchProvider searchProvider,
			IPersonRepository personRepository, IIanaTimeZoneProvider ianaTimeZoneProvider,
			IToggleManager toggleManager,
			IUserUiCulture userUiCulture,
			IScheduleDayProvider scheduleDayProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_teamScheduleProjectionProvider = teamScheduleProjectionProvider;
			_projectionProvider = projectionProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
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
				return new GroupScheduleViewModel
				{
					Schedules = new List<GroupScheduleShiftViewModel>(),
					Keyword = "",
					Total = 0
				};
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
			if (personIds == null || !personIds.Any() || scheduleDate.Date.Date == DateTime.MinValue)
			{
				return new GroupScheduleViewModel
				{
					Schedules = new List<GroupScheduleShiftViewModel>(),
					Total = 0
				};
			}
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			var people = _personRepository.FindPeople(personIds);
			var peopleCanSeeConfidentialAbsencesFor = people.Where(
				person => _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
					scheduleDate, person))
				.Select(person => person.Id.GetValueOrDefault()).ToArray();

			var previousDay = scheduleDate.AddDays(-1);
			var nextDay = scheduleDate.AddDays(1);

			var schedulesDictionary =
				_scheduleDayProvider.GetScheduleDictionary(scheduleDate, people, new ScheduleDictionaryLoadOptions(false, true));

			var list = new List<GroupScheduleShiftViewModel>();
			var agentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			var dates = new[] { scheduleDate, previousDay, nextDay };
			foreach (var person in people)
			{
				var personScheduleRange = schedulesDictionary[person];
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person.Id.GetValueOrDefault());
				list.AddRange(from date in dates
							  let scheduleDay = personScheduleRange.ScheduledDay(date)
							  where scheduleDay != null
							  select _teamScheduleProjectionProvider.MakeViewModel(person, date, scheduleDay, personScheduleRange.ScheduledDay(date.AddDays(-1)), canViewConfidential,
								  canSeeUnpublishedSchedules, date == scheduleDate, agentNameSetting));
			}

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = people.Count
			};
		}

	

		private static bool isSchedulePublished(DateOnly date, IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return false;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
				   workflowControlSet.SchedulePublishedToDate.Value >= date.Date;
		}
		private Tuple<IPerson, IScheduleDay>[] sortSchedules(Tuple<IPerson, IScheduleDay>[] personScheduleDaysToSort, TeamScheduleSortOption sortOption, bool canSeeUnpublishedSchedules)
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
					Array.Sort(personScheduleDaysToSort, new TeamScheduleComparer(canSeeUnpublishedSchedules, _permissionProvider));
					break;
			}
			return personScheduleDaysToSort;
		}
		private List<IPerson> getPermittedPersons(Guid[] targetIds, DateOnly date)
		{
			var matchedPersons = _personRepository.FindPeople(targetIds);
			return _searchProvider
				.GetPermittedPersonList(matchedPersons, date, DefinedRaptorApplicationFunctionPaths.ViewSchedules)
				.ToList();
		}

		private List<IPerson> getPermittedPersons(Guid[] targetIds, DateOnlyPeriod period)
		{
			var matchedPersons = _personRepository.FindPeople(targetIds);
			return _searchProvider
				.GetPermittedPersonList(matchedPersons, period, DefinedRaptorApplicationFunctionPaths.ViewSchedules)
				.ToList();
		}
		private GroupScheduleViewModel createViewModelForPeople(IList<Guid> targetIds, SearchDaySchedulesInput input)
		{
			var permittedPersons = new List<IPerson>();
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			foreach (var batch in targetIds.Batch(251))
			{
				var batchPermittedPersons = getPermittedPersons(batch.ToArray(), input.DateInUserTimeZone);
				if (input.IsOnlyAbsences)
				{
					var scheduleDaysForPerson =
						_scheduleProvider.GetScheduleForPersons(input.DateInUserTimeZone, batchPermittedPersons);
					batchPermittedPersons =
						scheduleDaysForPerson.Where(sd => (canSeeUnpublishedSchedules || sd.IsFullyPublished) && sd.HasAbsenceProjection()).Select(sd => sd.Person).ToList();
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

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider.GetPermittedPersonIdList(permittedPersons, input.DateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToList();


			var scheduleDays = _scheduleProvider.GetScheduleForPersons(input.DateInUserTimeZone, permittedPersons, true).ToArray();
			var scheduleDayLookup = scheduleDays.ToLookup(s => s.Person);


			if (input.IsOnlyAbsences)
			{
				permittedPersons = scheduleDays.Where(s =>
				{
					var personAbsences = s.PersonAbsenceCollection();
					var personAssignment = s.PersonAssignment();
					return personAssignment == null
						? personAbsences.Any()
						: personAbsences.Any(a => a.Period.ContainsPart(personAssignment.Period));
				}).Select(s => s.Person).ToList();
			}

			var personScheduleDaysToSort = permittedPersons.Select(s => new Tuple<IPerson, IScheduleDay>(s, scheduleDayLookup[s].FirstOrDefault())).ToArray();
			personScheduleDaysToSort = sortSchedules(personScheduleDaysToSort, input.SortOption, canSeeUnpublishedSchedules);

			var personScheduleDayPairsForCurrentPage = input.PageSize > 0
				? personScheduleDaysToSort.Skip(input.PageSize * (input.CurrentPageIndex - 1)).Take(input.PageSize).ToList()
				: personScheduleDaysToSort.ToList();

			var personsForCurrentPage = personScheduleDayPairsForCurrentPage.Select(pair => pair.Item1).ToList();

			var previousDay = input.DateInUserTimeZone.AddDays(-1);
			var scheduleDaysForPreviousDayLookup =
				_scheduleProvider.GetScheduleForPersons(previousDay, personsForCurrentPage).ToLookup(p => p.Person);
			var nextDay = input.DateInUserTimeZone.AddDays(1);
			var scheduleDaysForNextDayLookup =
				_scheduleProvider.GetScheduleForPersons(nextDay, personsForCurrentPage).ToLookup(p => p.Person);

			var list = new List<GroupScheduleShiftViewModel>();
			var agentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var pair in personScheduleDayPairsForCurrentPage)
			{
				var person = pair.Item1;
				var currentScheduleDay = pair.Item2;
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person.Id.GetValueOrDefault());
				var isPreviousDayScheduled = scheduleDaysForPreviousDayLookup.Contains(person);
				var schedulePreviousDay = isPreviousDayScheduled ? scheduleDaysForPreviousDayLookup[person].FirstOrDefault() : null;

				if (currentScheduleDay != null)
				{
					list.Add(_teamScheduleProjectionProvider.MakeViewModel(person, input.DateInUserTimeZone, currentScheduleDay, schedulePreviousDay, canViewConfidential, canSeeUnpublishedSchedules, true, agentNameSetting));
				}

				if (schedulePreviousDay != null)
				{
					list.Add(_teamScheduleProjectionProvider.MakeViewModel(person, previousDay, schedulePreviousDay,
						null, canViewConfidential, canSeeUnpublishedSchedules, false, agentNameSetting));
				}

				if (scheduleDaysForNextDayLookup.Contains(person))
				{
					list.Add(_teamScheduleProjectionProvider.MakeViewModel(person, nextDay, scheduleDaysForNextDayLookup[person].First(), currentScheduleDay, canViewConfidential, canSeeUnpublishedSchedules, false, agentNameSetting));
				}
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

			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;

			var pagedPeople = input.PageSize > 0
				? people.OrderBy(p => p.Name.LastName).Skip(input.PageSize * (input.CurrentPageIndex - 1)).Take(input.PageSize).ToList()
				: people.ToList();

			var scheduleDays = _scheduleProvider.GetScheduleForPersonsInPeriod(week.Inflate(1), pagedPeople)
				.ToDictionary(sd => new PersonDate
				{
					PersonId = sd.Person.Id.GetValueOrDefault(),
					Date = sd.DateOnlyAsPeriod.DateOnly
				});

			var viewableConfidentialAbsenceAgents =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, input.DateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential));

			var result = new List<PersonWeekScheduleViewModel>();

			foreach (var person in pagedPeople)
			{
				var daySchedules = weekDays
					.Select(d =>
					{
						var pd = new PersonDate
						{
							PersonId = person.Id.GetValueOrDefault(),
							Date = d
						};

						var dayScheduleViewModel = new PersonDayScheduleSummayViewModel
						{
							IsTerminated = person.TerminalDate.HasValue && person.TerminalDate.Value < d,
							Date = d,
							DayOfWeek = (int)d.DayOfWeek
						};

						if (!scheduleDays.TryGetValue(pd, out var scheduleDay) || dayScheduleViewModel.IsTerminated)
						{
							return dayScheduleViewModel;
						}

						var canViewConfidentialAbsence = viewableConfidentialAbsenceAgents[pd.Date].Contains(pd.PersonId);

						var isSchedulePublished = TeamScheduleViewModelFactory.isSchedulePublished(scheduleDay.DateOnlyAsPeriod.DateOnly, scheduleDay.Person);
						if (!isSchedulePublished && !canSeeUnpublishedSchedules) return dayScheduleViewModel;

						var significantPart = scheduleDay.SignificantPartForDisplay();
						var personAssignment = scheduleDay.PersonAssignment();
						var absenceCollection = scheduleDay.PersonAbsenceCollection();
						var visualLayerCollection = _projectionProvider.Projection(scheduleDay);

						if (visualLayerCollection != null && visualLayerCollection.HasLayers)
						{
							dayScheduleViewModel.ContractTimeMinutes = visualLayerCollection.ContractTime().TotalMinutes;
						}

						if (significantPart == SchedulePartView.DayOff)
						{
							dayScheduleViewModel.Title = personAssignment.DayOff().Description.Name;
							dayScheduleViewModel.IsDayOff = true;
						}
						else if (significantPart == SchedulePartView.MainShift)
						{
							dayScheduleViewModel.Title = personAssignment.ShiftCategory.Description.Name;
							var timeZone = scheduleDay.Person.PermissionInformation.DefaultTimeZone();
							dayScheduleViewModel.Timezone = new TimeZoneViewModel
							{
								IanaId = _ianaTimeZoneProvider.WindowsToIana(timeZone.Id),
								DisplayName = timeZone.DisplayName
							};
							dayScheduleViewModel.DateTimeSpan = scheduleDay.ProjectionService().CreateProjection().Period();

							if (personAssignment.ShiftCategory != null)
							{
								dayScheduleViewModel.Color =
									$"rgb({personAssignment.ShiftCategory.DisplayColor.R},{personAssignment.ShiftCategory.DisplayColor.G},{personAssignment.ShiftCategory.DisplayColor.B})";
							}
						}
						else if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
						{
							var absence = absenceCollection.OrderBy(a => a.Layer.Payload.Priority)
								.ThenByDescending(a => absenceCollection.IndexOf(a)).First().Layer.Payload;

							dayScheduleViewModel.IsDayOff = significantPart == SchedulePartView.ContractDayOff;

							if (absence.Confidential && !canViewConfidentialAbsence)
							{
								dayScheduleViewModel.Title = ConfidentialPayloadValues.Description.Name;
								dayScheduleViewModel.Color = ConfidentialPayloadValues.DisplayColorHex;
							}
							else
							{
								dayScheduleViewModel.Title = absence.Description.Name;
								dayScheduleViewModel.Color = $"rgb({absence.DisplayColor.R},{absence.DisplayColor.G},{absence.DisplayColor.B})";
							}
						}
						return dayScheduleViewModel;
					}).ToList();

				result.Add(new PersonWeekScheduleViewModel
				{
					PersonId = person.Id.GetValueOrDefault(),
					Name = nameDescriptionSetting.BuildFor(person),
					DaySchedules = daySchedules,
					ContractTimeMinutes = daySchedules.Sum(s => s.ContractTimeMinutes)
				});
			}

			return new GroupWeekScheduleViewModel
			{
				PersonWeekSchedules = result.ToList(),
				Total = people.Count
			};
		}

		private bool isResultTooMany(IList<IPerson> people)
		{
			var max = _toggleManager.IsEnabled(Toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871) ? 750 : 500;
			return people.Count > max;

		}

		private struct PersonDate
		{
			public Guid PersonId { get; set; }
			public DateOnly Date { get; set; }
		}
	}
}
