﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSchedulesForAllPeopleQueryHandler : IHandleQuery<GetSchedulesForAllPeopleQueryDto,ICollection<SchedulePartDto>>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly ISchedulePartAssembler _scheduleDayAssembler;

		public GetSchedulesForAllPeopleQueryHandler(IUnitOfWorkFactory unitOfWorkFactory, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IDateTimePeriodAssembler dateTimePeriodAssembler, ISchedulePartAssembler scheduleDayAssembler)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleDayAssembler = scheduleDayAssembler;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<SchedulePartDto> Handle(GetSchedulesForAllPeopleQueryDto query)
		{
			IList<SchedulePartDto> returnList = new List<SchedulePartDto>();

			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId));
			var datePeriod = new DateOnlyPeriod(new DateOnly(query.StartDate.DateTime), new DateOnly(query.EndDate.DateTime));
			var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);

			_dateTimePeriodAssembler.TimeZone = timeZone;
			_scheduleDayAssembler.SpecialProjection = query.SpecialProjection;
			_scheduleDayAssembler.TimeZone = timeZone;

			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				IScenario scenario = GetGivenScenarioOrDefault(query);
				var people = _personRepository.FindPeopleInOrganizationLight(datePeriod);

				IScheduleDictionary scheduleDictionary =
					_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(people),
					                                                   new ScheduleDictionaryLoadOptions(true, false), period, scenario);

				foreach (var person in people)
				{
					IScheduleRange scheduleRange = scheduleDictionary[person];
					foreach (DateOnly dateOnly in datePeriod.DayCollection())
					{
						IScheduleDay part = scheduleRange.ScheduledDay(dateOnly);
						//rk - ugly hack until ScheduleProjectionService is stateless (=not depended on schedulday in ctor)
						//when that's done - inject a IProjectionService instead.
						returnList.Add(_scheduleDayAssembler.DomainEntityToDto(part));
					}
				}
			}

			return returnList;
		}

		private IScenario GetGivenScenarioOrDefault(GetSchedulesForAllPeopleQueryDto query)
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

			if (scenario == null)
			{
				throw new FaultException("No default scenario or scenario with id '" + query.ScenarioId + "' was found.");
			}
			return scenario;
		}
	}
}