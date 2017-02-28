﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IClearScheduleEvents
	{
		void Execute(IDifferenceCollection<IPersistableScheduleData> scheduleDifference);
	}
}