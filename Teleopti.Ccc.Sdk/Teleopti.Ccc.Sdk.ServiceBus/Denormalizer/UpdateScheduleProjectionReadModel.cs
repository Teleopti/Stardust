using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class UpdateScheduleProjectionReadModel : IUpdateScheduleProjectionReadModel
	{
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScheduleChangedNotification _scheduleChangedNotification;
	    private readonly IServiceBus _serviceBus;
	    private bool _initialLoad;

		public UpdateScheduleProjectionReadModel(IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IScheduleRepository scheduleRepository, IScheduleChangedNotification scheduleChangedNotification, IServiceBus serviceBus)
		{
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_scheduleRepository = scheduleRepository;
			_scheduleChangedNotification = scheduleChangedNotification;
		    _serviceBus = serviceBus;
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
			var range = schedule[person];
			
			DateTimePeriod? actualPeriod;
			if (_initialLoad)
			{
				actualPeriod = range.TotalPeriod();
			}
			else
			{
				actualPeriod = period;
				_scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(dateOnlyPeriod, scenario, personId);
			}

			if (!actualPeriod.HasValue) return;

			dateOnlyPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
			foreach (var scheduleDay in range.ScheduledDayCollection(dateOnlyPeriod))
			{
				var date = scheduleDay.DateOnlyAsPeriod.DateOnly;

                if (!_initialLoad)
                {
                    _scheduleChangedNotification.Notify(scenario,person,date);
                }

				var projection = scheduleDay.ProjectionService().CreateProjection();
				foreach (var layer in projection)
				{
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, scenario, personId, layer, projection);
				}
			}

            _serviceBus.Send(new RTAUpdatedScheduleDayMessage()
                                                                {
                                                                    ActivityStartDateTime = period.StartDateTime,
                                                                    ActivityEndDateTime = period.EndDateTime,
                                                                    PersonId = personId
                                                                });
		}

		public void SetInitialLoad(bool initialLoad)
		{
			_initialLoad = initialLoad;
		}
	}
}