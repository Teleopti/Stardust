using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Query
{
	public class ScheduleByPersonIdHandler : IQueryHandler<ScheduleByPersonIdDto, ScheduleDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;

		public ScheduleByPersonIdHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository, IScheduleStorage scheduleStorage)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_scheduleStorage = scheduleStorage;
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
						Name = l.Payload.ConfidentialDescription(person).Name,
						StartTime = l.Period.StartDateTime,
						EndTime = l.Period.EndDateTime,
						PayloadId = l.Payload.Id.GetValueOrDefault(),
						IsAbsence = l.Payload is IAbsence
					}).ToArray(),
					PersonId = person.Id.GetValueOrDefault(),
					TimezoneId = person.PermissionInformation.DefaultTimeZone().Id
				}).ToArray()
			};
		}
	}
}