﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleProjectionProvider _teamScheduleProjectionProvider;
		private readonly IProjectionProvider _projectionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IUserCulture _userCulture;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, IScheduleProvider scheduleProvider, 
			ITeamScheduleProjectionProvider teamScheduleProjectionProvider, ILoggedOnUser loggedOnUser,
			ICommonAgentNameProvider commonAgentNameProvider, IPeopleSearchProvider searchProvider,
			IPersonRepository personRepository, IUserCulture userCulture, IProjectionProvider projectionProvider)
		{
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_teamScheduleProjectionProvider = teamScheduleProjectionProvider;
			_loggedOnUser = loggedOnUser;
			_commonAgentNameProvider = commonAgentNameProvider;
			_searchProvider = searchProvider;
			_personRepository = personRepository;
			_userCulture = userCulture;
			_projectionProvider = projectionProvider;
		}

		public GroupScheduleViewModel CreateViewModel(IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex, bool isOnlyAbsences)
		{
			var searchCriteria = _searchProvider.CreatePersonFinderSearchCriteria(criteriaDictionary, 9999, 1, dateInUserTimeZone,
				null);
			_searchProvider.PopulateSearchCriteriaResult(searchCriteria);
			var targetIds = searchCriteria.DisplayRows.Where(r => r.RowNumber > 0).Select(r => r.PersonId).Distinct();
			var matchedPersons = _personRepository.FindPeople(targetIds);
			var permittedPersons = _searchProvider.GetPermittedPersonList(matchedPersons,dateInUserTimeZone,DefinedRaptorApplicationFunctionPaths.ViewSchedules).ToArray();
					
			if (isOnlyAbsences)
			{
				permittedPersons = _searchProvider.SearchPermittedPeopleWithAbsence(permittedPersons, dateInUserTimeZone).ToArray();
				permittedPersons = filterAbsenceOutOfSchedule(dateInUserTimeZone,permittedPersons);
			}

			if (permittedPersons.Length > 500)
			{
				return new GroupScheduleViewModel
				{
					Schedules = new List<GroupScheduleShiftViewModel>(),
					Total = permittedPersons.Length,
				};
			}

			var peopleCanSeeConfidentialAbsencesFor = _searchProvider.GetPermittedPersonIdList(searchCriteria, dateInUserTimeZone,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential).ToArray();

			var list = constructGroupScheduleShiftViewModels(dateInUserTimeZone,permittedPersons, peopleCanSeeConfidentialAbsencesFor,
				pageSize, currentPageIndex);

			return new GroupScheduleViewModel
			{
				Schedules = list,
				Total = permittedPersons.Length
			};
		}

		public GroupWeekScheduleViewModel CreateWeekScheduleViewModel(
			IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex)
		{

			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(dateInUserTimeZone,_userCulture.GetCulture().DateTimeFormat.FirstDayOfWeek);
			var week = new DateOnlyPeriod(firstDayOfWeek,firstDayOfWeek.AddDays(6));

			var personIds = new HashSet<Guid>();
			var people = new List<IPerson>();

			foreach (var d in week.DayCollection())
			{
				var searchCriteria = _searchProvider.CreatePersonFinderSearchCriteria(criteriaDictionary,9999,1,d,null);
				_searchProvider.PopulateSearchCriteriaResult(searchCriteria);
				var targetIds = searchCriteria.DisplayRows.Where(r => r.RowNumber > 0).Select(r => r.PersonId).Where(id => !personIds.Contains(id)).ToList();

				if (targetIds.Count == 0) continue;

				var matchedPersons = _personRepository.FindPeople(targetIds);
				var permittedPersons = _searchProvider.GetPermittedPersonList(matchedPersons,d,DefinedRaptorApplicationFunctionPaths.ViewSchedules).ToList();
				permittedPersons.ForEach(p => personIds.Add(p.Id.GetValueOrDefault()));
				people.AddRange(permittedPersons);
			}
												
			if(people.Count > 500)
			{
				return new GroupWeekScheduleViewModel
				{
					PersonWeekSchedules = new List<PersonWeekScheduleViewModel>(),
					Total = people.Count,
				};
			}

			var list = constructPeopleWeekScheduleViewModel(dateInUserTimeZone, people, pageSize, currentPageIndex).ToList();

			return new GroupWeekScheduleViewModel
			{
				PersonWeekSchedules = list,
				Total = people.Count
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
				var personAbsences = scheduleDay.PersonAbsenceCollection();
				var personAssignment = scheduleDay.PersonAssignment();
				if (personAssignment == null)
				{
					if (personAbsences.Any())
					{
						ret.Add(scheduleDay.Person);
					}
				}
				else
				{
					var schedulePeriod = personAssignment.Period;
					var absenceInSchedulePeriod = personAbsences.Any(a => a.Period.Contains(schedulePeriod) || schedulePeriod.Contains(a.Period) || a.Period.Intersect(schedulePeriod));

					if (absenceInSchedulePeriod)
					{
						ret.Add(scheduleDay.Person);
					}
				}
			}
			return ret.ToArray();
		}

		private IEnumerable<PersonWeekScheduleViewModel> constructPeopleWeekScheduleViewModel(DateOnly dateInUserTimeZone,
			ICollection<IPerson> people, int pageSize, int currentPageIndex)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(dateInUserTimeZone, _userCulture.GetCulture().DateTimeFormat.FirstDayOfWeek);
			var week = new DateOnlyPeriod(firstDayOfWeek,firstDayOfWeek.AddDays(6));
			var weekDays = week.DayCollection();

			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;

			var pagedPeople = pageSize > 0
				? people.OrderBy(p => nameDescriptionSetting.BuildCommonNameDescription(p)).Skip(pageSize*(currentPageIndex - 1)).Take(pageSize).ToList()
				: people.ToList();

			var scheduleDays = _scheduleProvider.GetScheduleForPersonsInPeriod(week.Inflate(1), pagedPeople)
				.ToDictionary(sd => new PersonDate
				{
					PersonId = sd.Person.Id.GetValueOrDefault(),
					Date = sd.DateOnlyAsPeriod.DateOnly
				});

			var viewableConfidentialAbsenceAgents =
				weekDays.ToDictionary(d => d, d => _searchProvider.GetPermittedPersonIdList(pagedPeople, dateInUserTimeZone,
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

						if (!scheduleDays.ContainsKey(pd) || dayScheduleViewModel.IsTerminated)
						{
							return dayScheduleViewModel;
						}

						var scheduleDay = scheduleDays[pd];
						var canViewConfidentialAbsence = viewableConfidentialAbsenceAgents[pd.Date].Contains(pd.PersonId);

						completeScheduleDaySummary(dayScheduleViewModel, scheduleDay, canViewConfidentialAbsence, canSeeUnpublishedSchedules);

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

			return result;
		}	

		private IEnumerable<GroupScheduleShiftViewModel> constructGroupScheduleShiftViewModels(DateOnly dateInUserTimeZone,
			ICollection<IPerson> people, Guid[] peopleCanSeeConfidentialAbsencesFor, int pageSize, int currentPageIndex)
		{
			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var scheduleDays = _scheduleProvider.GetScheduleForPersons(dateInUserTimeZone, people, true).ToList();
			var scheduleDaysForPreviousDay =
				_scheduleProvider.GetScheduleForPersons(dateInUserTimeZone.AddDays(-1), people) ??
				new IScheduleDay[] {};
			scheduleDays.AddRange(
				scheduleDaysForPreviousDay.Where(
					scheduleDay =>
					{
						var personAssignment = scheduleDay?.PersonAssignment();
						return personAssignment != null &&
							   TimeZoneHelper.ConvertFromUtc(personAssignment.Period.EndDateTime, userTimeZone) >
							   dateInUserTimeZone.Date;
					}));

			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			var lookupSchedule = scheduleDays.ToLookup(s => s.Person);
			var personScheduleDaysToSort = people.Select(p =>
			{
				var schedule = lookupSchedule[p].SingleOrDefault(s => s.DateOnlyAsPeriod.DateOnly == dateInUserTimeZone);
				return new Tuple<IPerson, IScheduleDay>(p,schedule);
			}).ToArray();

			Array.Sort(personScheduleDaysToSort, new TeamScheduleComparer(canSeeUnpublishedSchedules,_permissionProvider));

			var requestedPersonScheduleDayPairs = pageSize > 0
				? personScheduleDaysToSort.Skip(pageSize*(currentPageIndex - 1)).Take(pageSize)
				: personScheduleDaysToSort;
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
			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			var list = new List<GroupScheduleShiftViewModel>();
			foreach (var personScheduleDay in requestedPersonScheduleDays)
			{
				var person = personScheduleDay.Person;
				var schedules = personScheduleDay.Schedules.ToArray();
				var canViewConfidential = peopleCanSeeConfidentialAbsencesFor.Contains(person.Id.GetValueOrDefault());
				if (schedules.All(s => s.DateOnlyAsPeriod.DateOnly != dateInUserTimeZone))
				{
					list.Add(new GroupScheduleShiftViewModel
					{
						PersonId = person.Id.GetValueOrDefault().ToString(),
						Name = nameDescriptionSetting.BuildCommonNameDescription(person),
						Date = dateInUserTimeZone.Date.ToFixedDateFormat(),
						Projection = new List<GroupScheduleProjectionViewModel>()
					});
				}

				foreach(var scheduleDay in schedules)
				{
					var isPublished = isSchedulePublished(scheduleDay.DateOnlyAsPeriod.DateOnly,person);
					var vm = isPublished || canSeeUnpublishedSchedules
						? _teamScheduleProjectionProvider.Projection(scheduleDay, canViewConfidential, nameDescriptionSetting)
						: new GroupScheduleShiftViewModel
						{
							PersonId = person.Id.GetValueOrDefault().ToString(),
							Name = nameDescriptionSetting.BuildCommonNameDescription(person),
							Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToFixedDateFormat(),
							Projection = new List<GroupScheduleProjectionViewModel>()
						};
					var note = scheduleDay.NoteCollection().FirstOrDefault();
					vm.InternalNotes = note != null && scheduleDay.DateOnlyAsPeriod.DateOnly == dateInUserTimeZone
						? note.GetScheduleNote(new NormalizeText())
						: string.Empty;
					list.Add(vm);
				}

			}
			return list;
		}

		private void completeScheduleDaySummary(PersonDayScheduleSummayViewModel vm, IScheduleDay scheduleDay,
			bool canViewConfidentialAbsence,
			bool canViewUnpublishedSchedules)
		{
			var isSchedulePublished = TeamScheduleViewModelFactory.isSchedulePublished(scheduleDay.DateOnlyAsPeriod.DateOnly, scheduleDay.Person);
			if (!isSchedulePublished && !canViewUnpublishedSchedules) return;

			var significantPart = scheduleDay.SignificantPartForDisplay();
			var personAssignment = scheduleDay.PersonAssignment();			
			var absenceCollection = scheduleDay.PersonAbsenceCollection();
			var visualLayerCollection = _projectionProvider.Projection(scheduleDay);

			if (visualLayerCollection != null && visualLayerCollection.HasLayers)
			{
				vm.ContractTimeMinutes = visualLayerCollection.ContractTime().TotalMinutes;
			}

			if (significantPart == SchedulePartView.DayOff)
			{
				vm.Title = personAssignment.DayOff().Description.Name;
				vm.IsDayOff = true;
			}
			else if (significantPart == SchedulePartView.MainShift)
			{
				vm.Title = personAssignment.ShiftCategory.Description.Name;
				vm.TimeSpan = personAssignment.PeriodExcludingPersonalActivity()
					.TimePeriod(scheduleDay.TimeZone);							

				if (personAssignment.ShiftCategory != null)
				{
					vm.Color =
						$"rgb({personAssignment.ShiftCategory.DisplayColor.R},{personAssignment.ShiftCategory.DisplayColor.G},{personAssignment.ShiftCategory.DisplayColor.B})";
				}				 
			}
			else if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
			{			
				var absence = absenceCollection.OrderBy(a => a.Layer.Payload.Priority)
					.ThenByDescending(a => absenceCollection.IndexOf(a)).First().Layer.Payload;

				vm.IsDayOff = significantPart == SchedulePartView.ContractDayOff;

				if (absence.Confidential && !canViewConfidentialAbsence)
				{
					vm.Title = ConfidentialPayloadValues.Description.Name;
					vm.Color = ConfidentialPayloadValues.DisplayColorHex;					
				}
				else
				{
					vm.Title = absence.Description.Name;
					vm.Color = $"rgb({absence.DisplayColor.R},{absence.DisplayColor.G},{absence.DisplayColor.B})";
				}				
			}
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
