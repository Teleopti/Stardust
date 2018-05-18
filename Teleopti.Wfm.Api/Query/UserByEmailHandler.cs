using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Query
{
	public class UserByEmailHandler : IQueryHandler<UserByEmailDto, UserDto>
	{
		private readonly IPersonRepository _personRepository;

		public UserByEmailHandler(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		[UnitOfWork]
		public virtual QueryResultDto<UserDto> Handle(UserByEmailDto command)
		{
			var people = _personRepository.FindPeopleByEmail(command.Email);
			return new QueryResultDto<UserDto>
			{
				Successful = true,
				Result = people.Select(p => new UserDto
				{
					FirstName = p.Name.FirstName,
					LastName = p.Name.LastName,
					Email = p.Email,
					Id = p.Id.GetValueOrDefault()
				}).ToArray()
			};
		}
	}

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
		public virtual QueryResultDto<ScheduleDto> Handle(ScheduleByPersonIdDto command)
		{
			var person = _personRepository.Get(command.PersonId);
			var period = new DateOnlyPeriod(new DateOnly(command.StartDate), new DateOnly(command.EndDate));
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
						Name = l.DisplayDescription().Name,
						StartTime = l.Period.StartDateTime,
						EndTime = l.Period.EndDateTime,
						PayloadId = l.Payload.Id.GetValueOrDefault()
					}).ToArray(),
					PersonId = person.Id.GetValueOrDefault()
				}).ToArray()
			};
		}
	}
}