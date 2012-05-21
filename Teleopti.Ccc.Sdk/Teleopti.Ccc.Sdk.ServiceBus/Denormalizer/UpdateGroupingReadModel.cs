using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class UpdateGroupingReadModel : IUpdateGroupingReadModel 
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		
        private readonly IScheduleChangedNotification _scheduleChangedNotification;
		
        private bool _skipDelete;

        public UpdateGroupingReadModel(IGroupingReadOnlyRepository groupingReadOnlyRepository,IScheduleChangedNotification scheduleChangedNotification)
		{
            _groupingReadOnlyRepository = groupingReadOnlyRepository;

			_scheduleChangedNotification = scheduleChangedNotification;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void Execute(IScenario scenario,DateTimePeriod period,IPerson person)
		{
            var timeZone = person.PermissionInformation.DefaultTimeZone();
            //var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			var schedule =
                _groupingReadOnlyRepository.AvailableGroupPages();

			
            //if (!_skipDelete)
            //{
            //    _scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(dateOnlyPeriod, scenario, personId);
            //}

            //var range = schedule[person];
            //var actualPeriod = range.TotalPeriod();
            //if (!actualPeriod.HasValue) return;

            //dateOnlyPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
            //foreach (var scheduleDay in schedule[person].ScheduledDayCollection(dateOnlyPeriod))
            //{
            //    var date = scheduleDay.DateOnlyAsPeriod.DateOnly;

                _scheduleChangedNotification.Notify(scenario,person,new DateOnly( ) );
				
            //    foreach (var layer in scheduleDay.ProjectionService().CreateProjection())
            //    {
                    //_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, scenario, personId, layer);
            //    }
            //}
		}

		public void SetSkipDelete(bool skipDelete)
		{
			_skipDelete = skipDelete;
		}
	}
}
