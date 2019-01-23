using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
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
			var currentUser = _loggedOnUser.CurrentUser();
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			return new QueryResultDto<ScheduleDto>
			{
				Successful = true,
				Result = scheduleDays.Select(p =>
				{
					return new ScheduleDto
					{
						Date = p.DateOnlyAsPeriod.DateOnly.Date,
						Shift = p.ProjectionService().CreateProjection().Select(l => new ShiftLayerDto
						{
							Name = l.Payload.ConfidentialDescription_DONTUSE(currentUser).Name,
							StartTime = l.Period.StartDateTime,
							EndTime = l.Period.EndDateTime,
							PayloadId = l.Payload.Id.GetValueOrDefault(),
							IsAbsence = l.Payload is IAbsence,
							DisplayColor = l.Payload.ConfidentialDisplayColor_DONTUSE(currentUser).ToArgb()
						}).ToArray(),
						PersonId = person.Id.GetValueOrDefault(),
						TimeZoneId = timeZone.Id
					};
				}).ToArray()
			};
		}
	}
}