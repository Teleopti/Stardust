using System;
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
	public class GetSchedulesByPersonQueryHandler : IHandleQuery<GetSchedulesByPersonQueryDto,ICollection<SchedulePartDto>>
	{
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly ISchedulePartAssembler _scheduleDayAssembler;

        public GetSchedulesByPersonQueryHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IDateTimePeriodAssembler dateTimePeriodAssembler, ISchedulePartAssembler scheduleDayAssembler)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleDayAssembler = scheduleDayAssembler;
		}

		public ICollection<SchedulePartDto> Handle(GetSchedulesByPersonQueryDto query)
		{
			IList<SchedulePartDto> returnList = new List<SchedulePartDto>();

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			var datePeriod = new DateOnlyPeriod(query.StartDate.ToDateOnly(), query.EndDate.ToDateOnly());
			var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));

			_dateTimePeriodAssembler.TimeZone = timeZone;
			_scheduleDayAssembler.SpecialProjection = query.SpecialProjection;
			_scheduleDayAssembler.TimeZone = timeZone;

			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				IScenario scenario = GetGivenScenarioOrDefault(query);
				var person = _personRepository.Get(query.PersonId);

				IScheduleDictionary scheduleDictionary =
					_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(true, false), period, scenario);

				IScheduleRange scheduleRange = scheduleDictionary[person];
				foreach (DateOnly dateOnly in datePeriod.DayCollection())
				{
					IScheduleDay part = scheduleRange.ScheduledDay(dateOnly);
					//rk - ugly hack until ScheduleProjectionService is stateless (=not depended on schedulday in ctor)
					//when that's done - inject a IProjectionService instead.
					returnList.Add(_scheduleDayAssembler.DomainEntityToDto(part));
				}
			}

			return returnList;
		}

		private IScenario GetGivenScenarioOrDefault(GetSchedulesByPersonQueryDto query)
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