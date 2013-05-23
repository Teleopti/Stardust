using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelHandler : 
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;

		public PersonScheduleDayReadModelHandler(
			IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
			IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository)
		{
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
		}

        private bool isDisabledForInitialLoad(ProjectionChangedEventBase message)
        {
            return message.IsInitialLoad;
        }

		private void createReadModel(ProjectionChangedEventBase message)
		{
			if (!message.IsDefaultScenario) return;
            if (isDisabledForInitialLoad(message)) return;
			if (message.ScheduleDays == null || message.ScheduleDays.Count == 0) return;

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(message.ScheduleDays.Min(s => s.Date.Date)),
			                                        new DateOnly(message.ScheduleDays.Max(s => s.Date.Date)));

			if (!message.IsInitialLoad)
			{
				_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
			}

			var readModels = _scheduleDayReadModelsCreator.GetReadModels(message);
			foreach (var readModel in readModels)
			{
				_scheduleDayReadModelRepository.SaveReadModel(readModel,notifyBroker: shouldNotifyBroker(message));
			}
		}

	    private static bool shouldNotifyBroker(ProjectionChangedEventBase message)
	    {
	        return !message.IsInitialLoad;
	    }

	    public void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			createReadModel(@event);
		}
	}

}