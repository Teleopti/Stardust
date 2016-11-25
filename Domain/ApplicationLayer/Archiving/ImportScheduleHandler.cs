using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Archiving
{
	[EnabledBy(Toggles.Wfm_ImportSchedule_41247)]
	public class ImportScheduleHandler :
		IHandleEvent<ImportScheduleEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUpdatedBy _updatedBy;
		private readonly ScheduleDictionaryLoadOptions _options = new ScheduleDictionaryLoadOptions(true, true);
		private readonly IJobResultRepository _jobResultRepository;

		public ImportScheduleHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository, IScheduleStorage scheduleStorage, ICurrentUnitOfWork currentUnitOfWork, IUpdatedBy updatedBy, IJobResultRepository jobResultRepository)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_scheduleStorage = scheduleStorage;
			_currentUnitOfWork = currentUnitOfWork;
			_updatedBy = updatedBy;
			_jobResultRepository = jobResultRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ImportScheduleEvent @event)
		{
			//_currentUnitOfWork.Current().Reassociate(_updatedBy.Person());
			//var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);

			//var fromScenario = _scenarioRepository.Get(@event.FromScenario);
			//var toScenario = _scenarioRepository.Get(@event.ToScenario);
			//var people = _personRepository.FindPeople(@event.PersonIds);

			//var targetScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, toScenario);
			//var sourceScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, fromScenario);
		}
	}
}