﻿using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherence
	{
		private readonly IApprovedPeriodsPersister _persister;

		public ApprovePeriodAsInAdherence(IApprovedPeriodsPersister persister)
		{
			_persister = persister;
		}

		public void Approve(ApprovedPeriod period)
		{
			new DateTimePeriod(period.StartTime, period.EndTime);
			_persister.Persist(period);
		}
	}
}