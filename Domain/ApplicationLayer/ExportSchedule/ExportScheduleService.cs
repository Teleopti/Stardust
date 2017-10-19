using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

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
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly IGroupPageRepository _groupPageRepository;


		public ExportScheduleService(IPeopleSearchProvider searchProvider, IPersonRepository personRepository, IPermissionProvider permissionProvider, ICommonAgentNameProvider commonAgentNameProvider, IScheduleDayProvider scheduleProvider, IScenarioRepository scenarioRepo, 
			IUserTextTranslator userTextTranslator, IOptionalColumnRepository optionalColumnRepository, IGroupPageRepository groupPageRepository)
		{
			_searchProvider = searchProvider;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
			_scheduleProvider = scheduleProvider;
			_scenarioRepo = scenarioRepo;
			_userTextTranslator = userTextTranslator;
			_optionalColumnRepository = optionalColumnRepository;
			_groupPageRepository = groupPageRepository;
		}

		public ProcessExportResult ExportToExcel(ExportScheduleForm input)
		{
			var processResult = new ProcessExportResult();

			var people = peopleToExport(input);
			if (people.Count > 1000)
			{
				processResult.FailReason = "We only allow exporting maximum 1000 agents, you're trying to export " + people.Count +
										   " agents."; //TODO

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
					.GetPermittedPersonList(matchedPersons, input.StartDate, DefinedRaptorApplicationFunctionPaths.ViewSchedules)
					.ToList();
				people.AddRange(permittedPeopleBatch);
			}
			return people;
		}
		private ScheduleExcelExportData prepareExportData(ExportScheduleForm input, IList<IPerson> peopleToExport)
		{
			var period = new DateOnlyPeriod(input.StartDate, input.EndDate);

			var timeZone = TimeZoneInfo.GetSystemTimeZones().Single(t => t.Id == input.TimezoneId);

			var scenario = _scenarioRepo.Load(input.ScenarioId);

			var selectedGroupNames = input.SelectedGroups.SelectedGroupIds;

			var scheduleDays = _scheduleProvider.GetScheduleDays(period, peopleToExport, scenario);

			var personRows = peopleToExport.Select(p => createPersonScheduleRow(p, scheduleDays.Where(sd => sd.Person.Id == p.Id).ToList(), input.OptionalColumnIds?.ToList() ?? new List<Guid>(), timeZone));

			var optionalColumnNames = "";
			//input.OptionalColumnIds.ForEach(oc =>
			//{
			//	optionalColumnNames = string.Join(",", _optionalColumnRepository.UniqueValuesOnColumn(oc));
			//});

			return new ScheduleExcelExportData
			{
				SelectedGroups = selectedGroupNames.ToString(),
				DateFrom = input.StartDate.Date,
				DateTo = input.EndDate.Date,
				Scenario = input.ScenarioName,
				OptionalColumns = optionalColumnNames,
				Timezone = timeZone.DisplayName,
				PersonRows = personRows.ToArray()
			};
		}

		private PersonRow createPersonScheduleRow(IPerson p, IList<IScheduleDay> scheduleDays, IList<Guid> optionalColIds, TimeZoneInfo timeZone)
		{
			var hasPermissionToViewUnpublished =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths
					.ViewUnpublishedSchedules);
			var hasPermissionToViewConfidential =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			var nameDescriptionSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			var name = nameDescriptionSetting.BuildCommonNameDescription(p);
			var optionalColumns = new List<string>();
			if (optionalColIds.Any())
			{
				optionalColIds.ForEach(o =>
				{
					if (p.OptionalColumnValueCollection.Any(v => v.Id.GetValueOrDefault() == o))
					{
						var optionalColumValue = p.OptionalColumnValueCollection.Single(x => x.Parent.Id == o).Description;
						optionalColumns.Add(optionalColumValue);
					}
				});
			}

			var scheduleSummarys = scheduleDays.Where(s => s.Person.Id == p.Id)
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
							scheduleDaySummary.Summary = ConfidentialPayloadValues.TranslatedDescription(_userTextTranslator).Name;
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
				SiteNTeam = p.MyTeam(scheduleDays.First().DateOnlyAsPeriod.DateOnly).SiteAndTeam,
				EmploymentNumber = p.EmploymentNumber,
				OptionalColumns = optionalColumns.ToArray(),
				ScheduleDaySummarys = scheduleSummarys.ToArray()
			};
		}
	}
}