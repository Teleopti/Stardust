﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public interface INotificationValidationCheck
	{
		void InitiateNotify(ScheduleDayReadModel readModel, DateOnly date, IPerson person);
	}
}