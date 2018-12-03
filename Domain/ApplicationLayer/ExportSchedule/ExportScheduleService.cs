using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule
{
	public class ExportScheduleService
	{
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IScheduleDayProvider _scheduleProvider;
		private readonly IScenarioRepository _scenarioRepo;
		private readonly IUserTextTranslator _userTextTranslator;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IOptionalColumnRepository _optionalColumnRepository;


		public ExportScheduleService(IPeopleSearchProvider searchProvider, IPersonRepository personRepository, IPermissionProvider permissionProvider, ICommonAgentNameProvider commonAgentNameProvider, IScheduleDayProvider scheduleProvider, IScenarioRepository scenarioRepo, 
			IUserTextTranslator userTextTranslator, IOptionalColumnRepository optionalColumnRepository, IGroupingReadOnlyRepository groupingReadOnlyRepository, ITeamRepository teamRepository)
		{
			_searchProvider = searchProvider;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
			_scheduleProvider = scheduleProvider;
			_scenarioRepo = scenarioRepo;
			_userTextTranslator = userTextTranslator;
			_optionalColumnRepository = optionalColumnRepository;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_teamRepository = teamRepository;
		}

		public ProcessExportResult ExportToExcel(ExportScheduleForm input)
		{
			var processResult = new ProcessExportResult();
			var people = peopleToExport(input);
			
			if (people.Count > 1000)
			{
				processResult.FailReason = string.Format(Resources.MaximumAgentToExport, people.Count);
				return processResult;
			}

			var exportData = prepareExportData(input, people);

			var excelCreator = new TeamsExcelCreator();
			var fileStream = excelCreator.CreateExcelFile(exportData);
			processResult.Data = fileStream;
			return processResult;
		}

		private IList<IPerson> peopleToExport(ExportScheduleForm input)
		{
			var period = new DateOnlyPeriod(input.StartDate, input.EndDate);
			var personIds = !input.SelectedGroups.IsDynamic
				? _searchProvider.FindPersonIdsInPeriodWithGroup(period, input.SelectedGroups.GroupIds,
					new Dictionary<PersonFinderField, string>())
				: _searchProvider.FindPersonIdsInPeriodWithDynamicGroup(period, input.SelectedGroups.SelectedGroupPageId,
					input.SelectedGroups.DynamicOptionalValues, new Dictionary<PersonFinderField, string>());
			var people = new List<IPerson>();
			foreach (var batch in personIds.Batch(500))
			{
				var matchedPersons = _personRepository.FindPeople(batch);
				var permittedPeopleBatch = _searchProvider
					.GetPermittedPersonList(matchedPersons, input.StartDate, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
				people.AddRange(permittedPeopleBatch);
			}
			return people;
		}
		private ScheduleExcelExportData prepareExportData(ExportScheduleForm input, IList<IPerson> peopleToExport)
		{
			var period = new DateOnlyPeriod(input.StartDate, input.EndDate);

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(input.TimezoneId);

			var scenario = _scenarioRepo.Load(input.ScenarioId);

			string selectedGroupNames;
			if (input.SelectedGroups.SelectedGroupPageId == Guid.Empty)
			{
				var selectedTeams = _teamRepository.FindTeams(input.SelectedGroups.GroupIds);
				selectedGroupNames = string.Join(",", selectedTeams.Select(t => t.SiteAndTeam));
			}
			else if (!input.SelectedGroups.IsDynamic)
			{
				var parentGroups = _groupingReadOnlyRepository.GetGroupPage(input.SelectedGroups.SelectedGroupPageId);
				var selectedGroups = _groupingReadOnlyRepository.FindGroups(input.SelectedGroups.GroupIds, period);
				selectedGroupNames = string.Join(",", selectedGroups.Select(g => _userTextTranslator.TranslateText(parentGroups.PageName) + "/" + g.GroupName));
			}
			else
			{
				var selectedGroupPage = _optionalColumnRepository.GetOptionalColumns<Person>().Single(p => p.AvailableAsGroupPage && p.Id == input.SelectedGroups.SelectedGroupPageId);
				var selectedGroups = _optionalColumnRepository.UniqueValuesOnColumn(selectedGroupPage.Id.GetValueOrDefault()).Where(ocv => input.SelectedGroups.DynamicOptionalValues.Contains(ocv.Description));
				selectedGroupNames = string.Join(",", selectedGroups.Select(g => selectedGroupPage.Name + "/" + g.Description));
			}
			var personRows = new PersonRow[0];
			var selectedOptionalColumns = input.OptionalColumnIds == null ? new List<IOptionalColumn>()
				: _optionalColumnRepository.GetOptionalColumns<Person>()
					.Where(p => input.OptionalColumnIds
						.Contains(p.Id.GetValueOrDefault())).OrderBy(t => t.Name).ToList();
			if (peopleToExport.Any())
			{
				var scheduleDayLookup = _scheduleProvider.GetScheduleDays(period, peopleToExport, scenario).ToLookup(x => x.Person);

				var peoplePermittedToViewUnpublished = _searchProvider.GetPermittedPersonIdList(peopleToExport, input.StartDate,
					DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
				var peoplePermittedToViewConfidential = _searchProvider.GetPermittedPersonIdList(peopleToExport, input.StartDate,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential);
				var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;

				personRows = scheduleDayLookup.Select(sl =>
						createPersonScheduleRow(sl, selectedOptionalColumns, timeZone, peoplePermittedToViewUnpublished.Contains(sl.Key.Id.GetValueOrDefault()), peoplePermittedToViewConfidential.Contains(sl.Key.Id.GetValueOrDefault()), nameDescriptionSetting))
					.OrderBy(r => r.Name).ToArray();
			}

			var optionalColumnNames = selectedOptionalColumns.Select(oc => oc.Name).ToArray();
			return new ScheduleExcelExportData
			{
				SelectedGroups = selectedGroupNames,
				DateFrom = input.StartDate.Date,
				DateTo = input.EndDate.Date,
				Scenario = scenario.Description.Name,
				OptionalColumns = optionalColumnNames,
				Timezone = timeZone.DisplayName,
				PersonRows = personRows
			};
		}
		
		private PersonRow createPersonScheduleRow(IGrouping<IPerson, IScheduleDay> personScheduleGroup, List<IOptionalColumn> selectedOptionalColumns, TimeZoneInfo timeZone, bool hasPermissionToViewUnpublished, bool hasPermissionToViewConfidential, ICommonNameDescriptionSetting nameDescriptionSetting)
		{
			var p = personScheduleGroup.Key;
			var scheduleDays = personScheduleGroup;
			var name = nameDescriptionSetting.BuildFor(p);
			var optionalColumns = new List<string>();

			selectedOptionalColumns.ForEach(o =>
			{
				var matchedOptionalColumValue = p.OptionalColumnValueCollection.SingleOrDefault(x => x.ReferenceObject.Id == o.Id);
				optionalColumns.Add(matchedOptionalColumValue != null ? matchedOptionalColumValue.Description : string.Empty);

			});

			var scheduleSummarys = scheduleDays
				.Select(sd =>
				{
					var scheduleDaySummary = new ScheduleDaySummary { Date = sd.DateOnlyAsPeriod.DateOnly };

					var isPublished = _permissionProvider.IsPersonSchedulePublished(sd.DateOnlyAsPeriod.DateOnly, p);
					if (!isPublished && !hasPermissionToViewUnpublished)
					{
						scheduleDaySummary.Summary = string.Empty;
						return scheduleDaySummary;
					}
					var significantPart = sd.SignificantPartForDisplay();
					var personAssignment = sd.PersonAssignment();
					var absenceCollection = sd.PersonAbsenceCollection();

					if (significantPart == SchedulePartView.DayOff)
					{
						scheduleDaySummary.Summary = personAssignment.DayOff().Description.Name;
					}
					else if (significantPart == SchedulePartView.MainShift)
					{
						scheduleDaySummary.Summary = personAssignment.ShiftCategory.Description.ShortName + " " + personAssignment.PeriodExcludingPersonalActivity().TimePeriod(timeZone).ToShortTimeString();
					}
					else if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
					{
						var absence = absenceCollection.OrderBy(a => a.Layer.Payload.Priority)
							.ThenByDescending(a => absenceCollection.IndexOf(a)).First().Layer.Payload;

						if (absence.Confidential && !hasPermissionToViewConfidential)
						{
							scheduleDaySummary.Summary = ConfidentialPayloadValues.Description.Name;
						}
						else
						{
							scheduleDaySummary.Summary = absence.Description.Name;
						}
					}
					return scheduleDaySummary;
				});

			return new PersonRow
			{
				Name = name,
				SiteNTeam = p.MyTeam(scheduleDays.First().DateOnlyAsPeriod.DateOnly)?.SiteAndTeam ?? string.Empty,
				EmploymentNumber = p.EmploymentNumber,
				OptionalColumns = optionalColumns.ToArray(),
				ScheduleDaySummarys = scheduleSummarys.ToArray()
			};
		}
	}
}