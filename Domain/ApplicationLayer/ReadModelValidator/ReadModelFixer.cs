using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ReadModelFixer : IReadModelFixer
	{
		private readonly IProjectionVersionPersister _projectionVersionPersister;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPersonScheduleDayReadModelPersister _personScheduleDayReadModelPersister;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		public ReadModelFixer(IProjectionVersionPersister projectionVersionPersister, IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, ICurrentScenario currentScenario, IPersonAssignmentRepository personAssignmentRepository, IPersonScheduleDayReadModelPersister personScheduleDayReadModelPersister, IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_projectionVersionPersister = projectionVersionPersister;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_currentScenario = currentScenario;
			_personAssignmentRepository = personAssignmentRepository;
			_personScheduleDayReadModelPersister = personScheduleDayReadModelPersister;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		public void FixScheduleProjectionReadOnly(ReadModelData data)
		{			
			var version =
				_projectionVersionPersister.LockAndGetVersions(data.PersonId,data.Date, data.Date).FirstOrDefault()?.Version;

			_scheduleProjectionReadOnlyPersister.BeginAddingSchedule(data.Date,_currentScenario.Current().Id.GetValueOrDefault(),
				data.PersonId,version ?? 0);		

			data.ScheduleProjectionReadOnly.ForEach(_scheduleProjectionReadOnlyPersister.AddActivity);
		}

		public void FixPersonScheduleDay(ReadModelData data)
		{			
			if (data.PersonScheduleDay == null)
			{
				_personScheduleDayReadModelPersister.DeleteReadModel(data.PersonId, data.Date);
				return;
			}
			data.PersonScheduleDay.ScheduleLoadTimestamp = _personAssignmentRepository.GetScheduleLoadedTime();
			_personScheduleDayReadModelPersister.SaveReadModel(data.PersonScheduleDay,false);
		}

		public void FixScheduleDay(ReadModelData data)
		{			
			if (data.ScheduleDay == null)
			{
				_scheduleDayReadModelRepository.ClearPeriodForPerson(new DateOnlyPeriod(data.Date, data.Date), data.PersonId);
				return;
			}
			_scheduleDayReadModelRepository.SaveReadModel(data.ScheduleDay);
		}

	}
}
