using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSchedulesByTeamQueryHandler : IHandleQuery<GetSchedulesByTeamQueryDto, ICollection<SchedulePartDto>>
	{
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly ISchedulePartAssembler _scheduleDayAssembler;

        public GetSchedulesByTeamQueryHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IDateTimePeriodAssembler dateTimePeriodAssembler, ISchedulePartAssembler scheduleDayAssembler)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleDayAssembler = scheduleDayAssembler;
		}

		public ICollection<SchedulePartDto> Handle(GetSchedulesByTeamQueryDto query)
		{
			var returnList = new List<SchedulePartDto>();

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			var datePeriod = new DateOnlyPeriod(query.StartDate.ToDateOnly(), query.EndDate.ToDateOnly());
			var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));

			_dateTimePeriodAssembler.TimeZone = timeZone;
			_scheduleDayAssembler.SpecialProjection = query.SpecialProjection;
			_scheduleDayAssembler.TimeZone = timeZone;
						
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				IScenario scenario = GetGivenScenarioOrDefault(query);

				var team = new Team();
				team.SetId(query.TeamId);

				var personList = _personRepository.FindPeopleBelongTeam(team, datePeriod);

				var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personList, new ScheduleDictionaryLoadOptions(false, false), period, scenario);
				foreach (IPerson person in personList)
				{
					var scheduleRange = scheduleDictionary[person];
					var parts = scheduleRange.ScheduledDayCollection(datePeriod);
					returnList.AddRange(_scheduleDayAssembler.DomainEntitiesToDtos(parts));
				}
			}

			return returnList;
		}

		private IScenario GetGivenScenarioOrDefault(GetSchedulesByTeamQueryDto query)
		{
			IScenario scenario;
			if (query.ScenarioId.HasValue)
			{
				scenario = _scenarioRepository.Get(query.ScenarioId.Value);
			}
			else
			{
				scenario = _scenarioRepository.LoadDefaultScenario();
			}

			if (scenario==null)
			{
				throw new FaultException("No default scenario or scenario with id '"+query.ScenarioId+"' was found.");
			}
			return scenario;
		}
	}
}