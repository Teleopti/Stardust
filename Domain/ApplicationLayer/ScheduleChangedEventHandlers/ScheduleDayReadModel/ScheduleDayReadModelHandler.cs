using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[EnabledBy(Toggles.ReadModel_ToHangfire_39147)]
	public class ScheduleDayReadModelHandlerHangfire :
		ScheduleDayReadModelHandlerBase,
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForScheduleDay>,
		IRunOnHangfire
	{
		public ScheduleDayReadModelHandlerHangfire(IPersonRepository personRepository, INotificationValidationCheck notificationValidationCheck, IScheduleDayReadModelsCreator scheduleDayReadModelsCreator, IScheduleDayReadModelRepository scheduleDayReadModelRepository) : base(personRepository, notificationValidationCheck, scheduleDayReadModelsCreator, scheduleDayReadModelRepository)
		{
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			CreateReadModel(@event);
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForScheduleDay @event)
		{
			CreateReadModel(@event);
		}
	}

	[EnabledBy(Toggles.ReadModel_ToHangfire_39147)]
	public class ScheduleDayReadModelHandlerServiceBus :
		ScheduleDayReadModelHandlerBase,
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForScheduleDay>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		public ScheduleDayReadModelHandlerServiceBus(IPersonRepository personRepository, INotificationValidationCheck notificationValidationCheck, IScheduleDayReadModelsCreator scheduleDayReadModelsCreator, IScheduleDayReadModelRepository scheduleDayReadModelRepository) : base(personRepository, notificationValidationCheck, scheduleDayReadModelsCreator, scheduleDayReadModelRepository)
		{
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			CreateReadModel(@event);
		}

		public void Handle(ProjectionChangedEventForScheduleDay @event)
		{
			CreateReadModel(@event);
		}
	}

	public abstract class ScheduleDayReadModelHandlerBase
	{
		private readonly IPersonRepository _personRepository;
		private readonly INotificationValidationCheck _notificationValidationCheck;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		protected ScheduleDayReadModelHandlerBase(IPersonRepository personRepository,
																			 INotificationValidationCheck notificationValidationCheck,
																			 IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
																			 IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_personRepository = personRepository;
			_notificationValidationCheck = notificationValidationCheck;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		protected void CreateReadModel(ProjectionChangedEventBase message)
		{
			if (!message.IsDefaultScenario) return;

			var person = _personRepository.Get(message.PersonId);

			foreach (var denormalizedScheduleDay in message.ScheduleDays)
			{
				var date = new DateOnly(denormalizedScheduleDay.Date);
				var dateOnlyPeriod = new DateOnlyPeriod(date, date);

				var readModel = _scheduleDayReadModelsCreator.GetReadModel(denormalizedScheduleDay, person);

				if (!message.IsInitialLoad)
				{
					try
					{
						_notificationValidationCheck.InitiateNotify(readModel, date, person);
					}
					catch (SendNotificationException)
					{
						// ignore it
					}
					_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
				}
				_scheduleDayReadModelRepository.SaveReadModel(readModel);
			}
		}
	}
}