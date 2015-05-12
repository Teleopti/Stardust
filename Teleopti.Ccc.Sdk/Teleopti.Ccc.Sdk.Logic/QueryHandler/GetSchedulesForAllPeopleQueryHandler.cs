﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSchedulesForAllPeopleQueryHandler : IHandleQuery<GetSchedulesForAllPeopleQueryDto,ICollection<SchedulePartDto>>
	{
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly ISchedulePartAssembler _scheduleDayAssembler;

        public GetSchedulesForAllPeopleQueryHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IDateTimePeriodAssembler dateTimePeriodAssembler, ISchedulePartAssembler scheduleDayAssembler)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleDayAssembler = scheduleDayAssembler;
		}

		public ICollection<SchedulePartDto> Handle(GetSchedulesForAllPeopleQueryDto query)
		{
			var returnList = new List<SchedulePartDto>();

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			var datePeriod = new DateOnlyPeriod(query.StartDate.ToDateOnly(), query.EndDate.ToDateOnly());
			var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));

			_dateTimePeriodAssembler.TimeZone = timeZone;
			_scheduleDayAssembler.SpecialProjection = query.SpecialProjection;
			_scheduleDayAssembler.TimeZone = timeZone;

			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				IScenario scenario = GetGivenScenarioOrDefault(query);
				var people = _personRepository.FindPeopleInOrganizationLight(datePeriod);

				IScheduleDictionary scheduleDictionary = _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(people, new ScheduleDictionaryLoadOptions(false, false), period, scenario);

				foreach (var person in people)
				{
					IScheduleRange scheduleRange = scheduleDictionary[person];
					var parts = scheduleRange.ScheduledDayCollection(datePeriod);
					returnList.AddRange(_scheduleDayAssembler.DomainEntitiesToDtos(parts));
				}
			}

			return returnList;
		}

		private IScenario GetGivenScenarioOrDefault(GetSchedulesForAllPeopleQueryDto query)
		{
			IScenario scenario = query.ScenarioId.HasValue
				? _scenarioRepository.Get(query.ScenarioId.Value)
				: _scenarioRepository.LoadDefaultScenario();

			if (scenario == null)
			{
				throw new FaultException("No default scenario or scenario with id '" + query.ScenarioId + "' was found.");
			}
			return scenario;
		}
	}
}