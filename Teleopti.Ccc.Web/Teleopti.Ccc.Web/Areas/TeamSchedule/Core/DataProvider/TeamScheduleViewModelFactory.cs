﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core;
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


		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, IScheduleProvider scheduleProvider,
			ITeamScheduleProjectionProvider teamScheduleProjectionProvider, IProjectionProvider projectionProvider,
			ICommonAgentNameProvider commonAgentNameProvider, IPeopleSearchProvider searchProvider,
			IPersonRepository personRepository, IIanaTimeZoneProvider ianaTimeZoneProvider, IToggleManager toggleManager)
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
		}


		public GroupScheduleViewModel CreateViewModel(Guid[] teamIds, IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex, bool isOnlyAbsences)
		{
			var targetIds = _toggleManager.IsEnabled(Toggles.Wfm_SearchAgentBasedOnCorrectPeriod_44552)
				? _searchProvider.FindPersonIdsInPeriod(new DateOnlyPeriod(dateInUserTimeZone, dateInUserTimeZone), teamIds, criteriaDictionary)
				: _searchProvider.FindPersonIds(dateInUserTimeZone, teamIds, criteriaDictionary);

			var permittedPersons = new List<IPerson>();
			foreach (var batch in targetIds.Batch(500))
			{
				var batchPermittedPersons = getPermittedPersons(batch.ToArray(), dateInUserTimeZone);
				if (isOnlyAbsences)
				{
					batchPermittedPersons = _searchProvider.SearchPermittedPeopleWithAbsence(batchPermittedPersons, dateInUserTimeZone).ToList();
				}
				permittedPersons.AddRange(batchPermittedPersons);
				if (permittedPersons.Count > 500)
				{
					return new GroupScheduleViewModel
					{
						Schedules = new List<GroupScheduleShiftViewModel>(),
						Total = targetIds.Count
					};
				}
			}

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider.GetPermittedPersonIdList(permittedPersons, dateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToList();
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var scheduleDays = _scheduleProvider.GetScheduleForPersons(dateInUserTimeZone, permittedPersons, true).ToArray();
			var scheduleDayLookup = scheduleDays.ToLookup(s => s.Person);


			if (isOnlyAbsences)
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
			Array.Sort(personScheduleDaysToSort, new TeamScheduleComparer(canSeeUnpublishedSchedules, _permissionProvider));

			var personScheduleDayPairsForCurrentPage = pageSize > 0
				? personScheduleDaysToSort.Skip(pageSize * (currentPageIndex - 1)).Take(pageSize).ToList()
				: personScheduleDaysToSort.ToList();

			var personsForCurrentPage = personScheduleDayPairsForCurrentPage.Select(pair => pair.Item1).ToList();

			var previousDay = dateInUserTimeZone.AddDays(-1);
			var scheduleDaysForPreviousDayLookup =
				_scheduleProvider.GetScheduleForPersons(previousDay, personsForCurrentPage).ToLookup(p => p.Person);
			var nextDay = dateInUserTimeZone.AddDays(1);
			var scheduleDaysForNextDayLookup =
				_scheduleProvider.GetScheduleForPersons(nextDay, personsForCurrentPage).ToLookup(p => p.Person);

			var list = new List<GroupScheduleShiftViewModel>();
			var agentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var pair in personScheduleDayPairsForCurrentPage)
			{
				var person = pair.Item1;
				var currentScheduleDay = pair.Item2;
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person.Id.GetValueOrDefault());

				list.Add(_teamScheduleProjectionProvider.MakeViewModel(person, dateInUserTimeZone, currentScheduleDay, canViewConfidential, canSeeUnpublishedSchedules, true, agentNameSetting));
				if (scheduleDaysForPreviousDayLookup.Contains(person))
				{
					list.AddRange(scheduleDaysForPreviousDayLookup[person]
						.Select(sd => _teamScheduleProjectionProvider.MakeViewModel(person, previousDay, sd, canViewConfidential, canSeeUnpublishedSchedules, false, agentNameSetting)));
				}
				if (scheduleDaysForNextDayLookup.Contains(person))
				{
					list.AddRange(scheduleDaysForNextDayLookup[person]
						.Select(sd => _teamScheduleProjectionProvider.MakeViewModel(person, nextDay, sd, canViewConfidential, canSeeUnpublishedSchedules, false, agentNameSetting)));
				}
			}

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = permittedPersons.Count
			};
		}

		public GroupWeekScheduleViewModel CreateWeekScheduleViewModel(
			Guid[] teamIds,
			IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly startOfWeekInUserTimeZone, int pageSize, int currentPageIndex)
		{
			var week = new DateOnlyPeriod(startOfWeekInUserTimeZone, startOfWeekInUserTimeZone.AddDays(6));

			var weekDays = week.DayCollection();
			var people = new List<IPerson>();
			var targetIds = new List<Guid>();

			if (_toggleManager.IsEnabled(Toggles.Wfm_SearchAgentBasedOnCorrectPeriod_44552))
			{
				targetIds = _searchProvider.FindPersonIdsInPeriod(week, teamIds, criteriaDictionary);
			}
			else
			{
				foreach (var d in weekDays)
				{
					targetIds.AddRange(_searchProvider.FindPersonIds(d, teamIds, criteriaDictionary));
				}
				targetIds = targetIds.Distinct().ToList();
			}

			foreach (var batch in targetIds.Batch(500))
			{
				var batchPermittedPersons = getPermittedPersons(batch.ToArray(), week.StartDate);
				people.AddRange(batchPermittedPersons);
				if (people.Count > 500)
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

			var pagedPeople = pageSize > 0
				? people.OrderBy(p => p.Name.LastName).Skip(pageSize * (currentPageIndex - 1)).Take(pageSize).ToList()
				: people.ToList();

			var scheduleDays = _scheduleProvider.GetScheduleForPersonsInPeriod(week.Inflate(1), pagedPeople)
				.ToDictionary(sd => new PersonDate
				{
					PersonId = sd.Person.Id.GetValueOrDefault(),
					Date = sd.DateOnlyAsPeriod.DateOnly
				});

			var viewableConfidentialAbsenceAgents =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, startOfWeekInUserTimeZone,
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

						IScheduleDay scheduleDay;
						if (!scheduleDays.TryGetValue(pd, out scheduleDay) || dayScheduleViewModel.IsTerminated)
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
							dayScheduleViewModel.Timezone = new TimeZoneViewModel
							{
								IanaId = _ianaTimeZoneProvider.WindowsToIana(scheduleDay.Person.PermissionInformation.DefaultTimeZone().Id),
								DisplayName = scheduleDay.Person.PermissionInformation.DefaultTimeZone().DisplayName
							};
							dayScheduleViewModel.DateTimeSpan = personAssignment.PeriodExcludingPersonalActivity();

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
					Name = nameDescriptionSetting.BuildCommonNameDescription(person),
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

		private List<IPerson> getPermittedPersons(Guid[] targetIds, DateOnly date)
		{
			var matchedPersons = _personRepository.FindPeople(targetIds);
			return _searchProvider
				.GetPermittedPersonList(matchedPersons, date, DefinedRaptorApplicationFunctionPaths.ViewSchedules)
				.ToList();
		}

		public GroupScheduleViewModel CreateViewModelForPeople(Guid[] personIds, DateOnly scheduleDate)
		{
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			var people = _personRepository.FindPeople(personIds);
			var peopleCanSeeConfidentialAbsencesFor = people.Where(
				person => _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
					scheduleDate, person))
				.Select(person => person.Id.GetValueOrDefault()).ToArray();

			var scheduleDays = _scheduleProvider.GetScheduleForPersons(scheduleDate, people, true).ToLookup(d => d.Person);
			//var personScheduleDaysToSort = scheduleDays.Select(s => new Tuple<IPerson,IScheduleDay>(s.Person,s)).ToArray();
			//Array.Sort(personScheduleDaysToSort,new TeamScheduleComparer(canSeeUnpublishedSchedules,_permissionProvider));
			var previousDay = scheduleDate.AddDays(-1);
			var scheduleDaysForPreviousDayLookup =
				_scheduleProvider.GetScheduleForPersons(scheduleDate.AddDays(-1), people).ToLookup(p => p.Person);
			var nextDay = scheduleDate.AddDays(1);
			var scheduleDaysForNextDayLookup =
				_scheduleProvider.GetScheduleForPersons(nextDay, people).ToLookup(p => p.Person);

			var list = new List<GroupScheduleShiftViewModel>();
			var agentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var person in people)
			{
				var currentScheduleDay = scheduleDays[person].SingleOrDefault();
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person.Id.GetValueOrDefault());

				list.Add(_teamScheduleProjectionProvider.MakeViewModel(person, scheduleDate, currentScheduleDay, canViewConfidential, canSeeUnpublishedSchedules, true, agentNameSetting));
				if (scheduleDaysForPreviousDayLookup.Contains(person))
				{
					list.AddRange(scheduleDaysForPreviousDayLookup[person]
						.Select(sd => _teamScheduleProjectionProvider.MakeViewModel(person, previousDay, sd, canViewConfidential, canSeeUnpublishedSchedules, false, agentNameSetting)));
				}
				if (scheduleDaysForNextDayLookup.Contains(person))
				{
					list.AddRange(scheduleDaysForNextDayLookup[person]
						.Select(sd => _teamScheduleProjectionProvider.MakeViewModel(person, nextDay, sd, canViewConfidential, canSeeUnpublishedSchedules, false, agentNameSetting)));
				}
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

		private struct PersonDate
		{
			public Guid PersonId { get; set; }
			public DateOnly Date { get; set; }
		}
	}
}
