using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSchedulesByChangeDateQueryHandler : IHandleQuery<GetSchedulesByChangeDateQueryDto, ScheduleChangesDto>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly ISchedulePartAssembler _scheduleDayAssembler;
		private readonly IPersonDayProjectionChangedRepository _personDayProjectionChangedRepository;
		private readonly ICurrentAuthorization _currentAuthorization;

		public GetSchedulesByChangeDateQueryHandler(
			IPersonDayProjectionChangedRepository personDayProjectionChangedRepository,
			ICurrentUnitOfWorkFactory unitOfWorkFactory, IScheduleStorage scheduleStorage, IPersonRepository personRepository,
			IScenarioRepository scenarioRepository, IDateTimePeriodAssembler dateTimePeriodAssembler,
			ISchedulePartAssembler scheduleDayAssembler,
			ICurrentAuthorization currentAuthorization)
		{
			_personDayProjectionChangedRepository = personDayProjectionChangedRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleDayAssembler = scheduleDayAssembler;
			_currentAuthorization = currentAuthorization;
		}

		public ScheduleChangesDto Handle(GetSchedulesByChangeDateQueryDto query)
		{
			validatePermissions();
			validateQuery(query);
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var utcNow = DateTime.UtcNow;
				if (query.ChangesToUTC <= query.ChangesFromUTC || query.ChangesToUTC > utcNow) // If to date is <= we 'ignore' it and read as much data as possible.
				{
					query.ChangesToUTC = utcNow;
				}
				// Load changes in absence and personassignment from a certain point in time for default scenario of authenticated SDK consumer.
				var authUserDefaultScenario = _scenarioRepository.LoadDefaultScenario();
				var personAssignmentChangeDays = _personDayProjectionChangedRepository.LoadPersonDayAssignmentChanges(query.ChangesFromUTC, query.ChangesToUTC, authUserDefaultScenario); // Agents local "day"
				var personAbsenceChangeDays = _personDayProjectionChangedRepository.LoadPersonDayAbsenceChanges(query.ChangesFromUTC, query.ChangesToUTC, authUserDefaultScenario); // UTC for start and end (convert to agent local)

				// Get Person data from personId's
				var personIdList = personAssignmentChangeDays.Select(p => p.PersonId).ToList();
				personIdList.AddRange(personAbsenceChangeDays.Select(p => p.PersonId));
				var peopleWithChanges = _personRepository.FindPeople(personIdList.Distinct()).ToDictionary(p => p.Id.Value);

				// Convert absence to agent locale AND optimize ranges
				var personDayProjectionChanges = new List<PersonDayProjectionChanged>(personAbsenceChangeDays.Count + personAssignmentChangeDays.Count);
				personDayProjectionChanges.AddRange(personAbsenceChangeDays.Select(a => a.Convert(peopleWithChanges[a.PersonId].PermissionInformation.DefaultTimeZone())));
				personDayProjectionChanges.AddRange(personAssignmentChangeDays);
				var optimizedChangedDayRangesPage = DayRangeOptimizer.ReduceAndPage(personDayProjectionChanges, query.Page, query.PageSize);

				var loadOptions = new ScheduleDictionaryLoadOptions(false, true, true)	{	LoadDaysAfterLeft = false	};
				var querySchedules = new List<SchedulePartDto>();
				foreach (var change in optimizedChangedDayRangesPage.Projections)
				{
					var person = peopleWithChanges[change.PersonId]; 
					var doPeriod = new DateOnlyPeriod(change.StartDate, change.EndDate);
					var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, loadOptions, doPeriod, authUserDefaultScenario);
					var scheduleRange = scheduleDictionary[person];
					var parts = scheduleRange.ScheduledDayCollection(doPeriod);
					querySchedules.AddRange(_scheduleDayAssembler.DomainEntitiesToDtos(parts));
				}

				return new ScheduleChangesDto
				{
					Schedules = querySchedules,
					TotalSchedules = optimizedChangedDayRangesPage.TotalSchedules,
					Page = query.Page,
					TotalPages = optimizedChangedDayRangesPage.TotalPages,
					ChangesUpToUTC = query.ChangesToUTC
				};
			}
		}

		private void validatePermissions()
		{
			var principalAuthorization = _currentAuthorization.Current();
			if (!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions))
			{
				throw new FaultException("This function requires higher permissions.");
			}
		}

		private void validateQuery(GetSchedulesByChangeDateQueryDto query)
		{
			if (query.Page < 0)
			{
				throw new FaultException($"Invalid query. '{nameof(query.Page)}'-parameter must be larger or equal to '0', (was: '{query.Page}').");
			}
			else if (query.Page >= 1 && query.PageSize <= 0)
			{
				throw new FaultException($"Invalid query. '{nameof(query.PageSize)}'-parameter must be larger than '0' when paging data, (was: '{query.PageSize}').");
			}

			if (query.ChangesFromUTC == DateTime.MinValue)
			{
				throw new FaultException($"Invalid query. '{nameof(query.ChangesFromUTC)}'-parameter must be specified, (was: '{query.ChangesFromUTC}').");
			}
		}
	}
}