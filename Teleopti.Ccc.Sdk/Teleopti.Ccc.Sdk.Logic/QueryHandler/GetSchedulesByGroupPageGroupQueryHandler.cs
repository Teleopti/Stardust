using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSchedulesByGroupPageGroupQueryHandler : IHandleQuery<GetSchedulesByGroupPageGroupQueryDto, ICollection<SchedulePartDto>>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly ISchedulePartAssembler _scheduleDayAssembler;

		public GetSchedulesByGroupPageGroupQueryHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IGroupingReadOnlyRepository groupingReadOnlyRepository, IDateTimePeriodAssembler dateTimePeriodAssembler, ISchedulePartAssembler scheduleDayAssembler)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleDayAssembler = scheduleDayAssembler;
		}

		public ICollection<SchedulePartDto> Handle(GetSchedulesByGroupPageGroupQueryDto query)
		{
			var returnList = new List<SchedulePartDto>();

			var queryDate = query.QueryDate.ToDateOnly();
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			var datePeriod = new DateOnlyPeriod(queryDate, queryDate);
			var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));

			_dateTimePeriodAssembler.TimeZone = timeZone;
			_scheduleDayAssembler.TimeZone = timeZone;
						
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				IScenario scenario = GetGivenScenarioOrDefault(query);

				var details = _groupingReadOnlyRepository.DetailsForGroup(query.GroupPageGroupId, queryDate);

				var availableDetails = details.Where(
					p => PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
																					  queryDate, p));

				var personList = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));

				IScheduleDictionary scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personList, new ScheduleDictionaryLoadOptions(false, false), period, scenario);
				foreach (IPerson person in personList)
				{
					IScheduleRange scheduleRange = scheduleDictionary[person];
					var parts = scheduleRange.ScheduledDayCollection(datePeriod);
					returnList.AddRange(_scheduleDayAssembler.DomainEntitiesToDtos(parts));
				}
			}

			return returnList;
		}

		private IScenario GetGivenScenarioOrDefault(GetSchedulesByGroupPageGroupQueryDto query)
		{
			IScenario scenario = query.ScenarioId.HasValue
				? _scenarioRepository.Get(query.ScenarioId.Value)
				: _scenarioRepository.LoadDefaultScenario();

			if (scenario==null)
			{
				throw new FaultException("No default scenario or scenario with id '"+query.ScenarioId+"' was found.");
			}
			return scenario;
		}
	}
}