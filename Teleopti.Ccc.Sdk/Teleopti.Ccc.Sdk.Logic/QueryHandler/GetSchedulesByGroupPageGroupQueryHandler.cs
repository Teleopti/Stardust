using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSchedulesByGroupPageGroupQueryHandler : IHandleQuery<GetSchedulesByGroupPageGroupQueryDto, ICollection<SchedulePartDto>>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly ISchedulePartAssembler _scheduleDayAssembler;

		public GetSchedulesByGroupPageGroupQueryHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IGroupingReadOnlyRepository groupingReadOnlyRepository, IDateTimePeriodAssembler dateTimePeriodAssembler, ISchedulePartAssembler scheduleDayAssembler)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scheduleDayAssembler = scheduleDayAssembler;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<SchedulePartDto> Handle(GetSchedulesByGroupPageGroupQueryDto query)
		{
			IList<SchedulePartDto> returnList = new List<SchedulePartDto>();

			var queryDate = query.QueryDate.ToDateOnly();
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId));
			var datePeriod = new DateOnlyPeriod(queryDate, queryDate);
			var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));

			_dateTimePeriodAssembler.TimeZone = timeZone;
			_scheduleDayAssembler.TimeZone = timeZone;
						
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				IScenario scenario = GetGivenScenarioOrDefault(query);

				var details = _groupingReadOnlyRepository.DetailsForGroup(query.GroupPageGroupId, queryDate);

				var availableDetails = details.Where(
					p => PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
																					  queryDate, p));

				var personList = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));

				IScheduleDictionary scheduleDictionary = _scheduleRepository.FindSchedulesOnlyForGivenPeriodAndPersons(personList, new ScheduleDictionaryLoadOptions(true, false), period, scenario);
				foreach (IPerson person in personList)
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

		private IScenario GetGivenScenarioOrDefault(GetSchedulesByGroupPageGroupQueryDto query)
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