using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Notification;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonScheduleChangeMessageRepository
	{
		void Add(PersonScheduleChangeMessage scheduleChangeMessage);
		IEnumerable<PersonScheduleChangeMessage> PopMessages(Guid userId);
	}
}
