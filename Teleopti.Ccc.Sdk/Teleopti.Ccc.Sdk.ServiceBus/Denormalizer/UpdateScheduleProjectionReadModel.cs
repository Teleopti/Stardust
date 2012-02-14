using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class UpdateScheduleProjectionReadModel : IUpdateScheduleProjectionReadModel
	{
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private bool _skipDelete;

		public UpdateScheduleProjectionReadModel(IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IScheduleRepository scheduleRepository)
		{
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_scheduleRepository = scheduleRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void Execute(IScenario scenario,DateTimePeriod period,IPerson person)
		{
            var timeZone = person.PermissionInformation.DefaultTimeZone();
            var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			var schedule =
				_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] {person}) {DoLoadByPerson = true},
                                                                   new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod.ToDateTimePeriod(timeZone), scenario);

			var personId = person.Id.GetValueOrDefault();

			if (!_skipDelete)
			{
				_scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(dateOnlyPeriod, scenario, personId);
			}

			var range = schedule[person];
			var actualPeriod = range.TotalPeriod();
			if (!actualPeriod.HasValue) return;

			dateOnlyPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
			foreach (var scheduleDay in schedule[person].ScheduledDayCollection(dateOnlyPeriod))
			{
				foreach (var layer in scheduleDay.ProjectionService().CreateProjection())
				{
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(scheduleDay.DateOnlyAsPeriod.DateOnly, scenario, personId, layer);
				}
			}
		}

		public void SetSkipDelete(bool skipDelete)
		{
			_skipDelete = skipDelete;
		}
	}
}