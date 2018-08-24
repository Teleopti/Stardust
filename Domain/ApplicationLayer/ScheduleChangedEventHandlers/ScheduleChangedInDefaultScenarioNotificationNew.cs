﻿using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedInDefaultScenarioNotificationNew : 
		IHandleEvent<ScheduleChangedEvent>, 
		IRunOnHangfire
	{
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IScenarioRepository _scenarioRepository;

		public ScheduleChangedInDefaultScenarioNotificationNew(IMessageBrokerComposite messageBroker, IScenarioRepository scenarioRepository)
		{
			_messageBroker = messageBroker;
			_scenarioRepository = scenarioRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ScheduleChangedEvent message)
		{
			if (!(_scenarioRepository.Get(message.ScenarioId)?.DefaultScenario ?? false)) return;
			var firstDate = message.StartDateTime;
			var lastDate = message.EndDateTime;
			_messageBroker.Send(
				message.LogOnDatasource, 
				message.LogOnBusinessUnitId, 
				firstDate, 
				lastDate, 
				Guid.Empty,
				message.PersonId, 
				typeof (Person), 
				Guid.Empty, 
				typeof (IScheduleChangedInDefaultScenario),
				DomainUpdateType.NotApplicable, 
				null,
				message.CommandId == Guid.Empty ? Guid.NewGuid(): message.CommandId);
		}
	}
}