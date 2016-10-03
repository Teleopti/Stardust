﻿using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	[EnabledBy(Toggles.ReadModel_ToHangfire_39147)]
	public class PersonScheduleDayReadModelUpdaterHangfire :
		PersonScheduleDayReadModelUpdaterBase,
		IRunOnHangfire
	{
		public PersonScheduleDayReadModelUpdaterHangfire(IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator, IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository, ITrackingMessageSender trackingMessageSender) : base(scheduleDayReadModelsCreator, scheduleDayReadModelRepository, trackingMessageSender)
		{
		}
	}

	[DisabledBy(Toggles.ReadModel_ToHangfire_39147)]
	public class PersonScheduleDayReadModelUpdaterServiceBus :
		PersonScheduleDayReadModelUpdaterBase,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		public PersonScheduleDayReadModelUpdaterServiceBus(IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator, IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository, ITrackingMessageSender trackingMessageSender) : base(scheduleDayReadModelsCreator, scheduleDayReadModelRepository, trackingMessageSender)
		{
		}
	}

	public abstract class PersonScheduleDayReadModelUpdaterBase :
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;
		private readonly ITrackingMessageSender _trackingMessageSender;

		private static readonly ILog logger = LogManager.GetLogger(typeof(PersonScheduleDayReadModelUpdaterBase));

		protected PersonScheduleDayReadModelUpdaterBase(
			IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
			IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository,
			ITrackingMessageSender trackingMessageSender)
		{
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_trackingMessageSender = trackingMessageSender;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
			if (_trackingMessageSender != null && @event.CommandId != Guid.Empty)
				_trackingMessageSender.SendTrackingMessage(@event, new TrackingMessage
				{
					TrackId = @event.CommandId,
					Status = TrackingMessageStatus.Success
				});
		}

		public void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			createReadModel(@event);
		}

		private void createReadModel(ProjectionChangedEventBase message)
		{
			if (logger.IsDebugEnabled)
				logger.Debug($"Updating model PersonScheduleDayReadModel for person {message.PersonId}");

			if (!message.IsDefaultScenario)
			{
				if (logger.IsDebugEnabled)
					logger.Debug("Skipping update of model PersonScheduleDayReadModel because its not in default scenario");
				return;
			}
			if (message.ScheduleDays == null || message.ScheduleDays.Count == 0)
			{
				if (logger.IsDebugEnabled)
					logger.Debug("Skipping update of model PersonScheduleDayReadModel because the event did not contain any days");
				return;
			}

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(message.ScheduleDays.Min(s => s.Date.Date)),
			                                        new DateOnly(message.ScheduleDays.Max(s => s.Date.Date)));

			var readModels = _scheduleDayReadModelsCreator.MakeReadModels(message);
			_scheduleDayReadModelRepository.UpdateReadModels(dateOnlyPeriod, message.PersonId, message.LogOnBusinessUnitId, readModels, initialLoad: message.IsInitialLoad);
		}
	}
}