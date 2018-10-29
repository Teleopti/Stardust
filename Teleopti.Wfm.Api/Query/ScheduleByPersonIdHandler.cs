﻿using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Api.Query.Request;
using Teleopti.Wfm.Api.Query.Response;

namespace Teleopti.Wfm.Api.Query
{
	public class ScheduleByPersonIdHandler : IQueryHandler<ScheduleByPersonIdDto, ScheduleDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ILoggedOnUser _loggedOnUser;

		public ScheduleByPersonIdHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository, IScheduleStorage scheduleStorage, ILoggedOnUser loggedOnUser)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_scheduleStorage = scheduleStorage;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork]
		public virtual QueryResultDto<ScheduleDto> Handle(ScheduleByPersonIdDto query)
		{
			var person = _personRepository.Get(query.PersonId);
			var period = new DateOnlyPeriod(new DateOnly(query.StartDate), new DateOnly(query.EndDate));
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), period, _scenarioRepository.LoadDefaultScenario());
			var scheduleDays = schedule[person].ScheduledDayCollection(period);
			return new QueryResultDto<ScheduleDto>
			{
				Successful = true,
				Result = scheduleDays.Select(p => new ScheduleDto
				{
					Date = p.DateOnlyAsPeriod.DateOnly.Date,
					Shift = p.ProjectionService().CreateProjection().Select(l => new ShiftLayerDto
					{
						Name = l.Payload.ConfidentialDescription(_loggedOnUser.CurrentUser()).Name,
						StartTime = l.Period.StartDateTime,
						EndTime = l.Period.EndDateTime,
						PayloadId = l.Payload.Id.GetValueOrDefault(),
						IsAbsence = l.Payload is IAbsence,
						DisplayColor = l.Payload.ConfidentialDisplayColor(_loggedOnUser.CurrentUser()).ToArgb()
					}).ToArray(),
					PersonId = person.Id.GetValueOrDefault(),
					TimeZoneId = person.PermissionInformation.DefaultTimeZone().Id
				}).ToArray()
			};
		}
	}
}