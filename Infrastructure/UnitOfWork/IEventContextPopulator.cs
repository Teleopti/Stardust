﻿using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IEventContextPopulator
	{
		void PopulateEventContext(IEvent @event);
		void PopulateEventContext(ILogOnInfo @event);
		void PopulateEventContextWithoutInitiator(ILogOnInfo message);
	}
}