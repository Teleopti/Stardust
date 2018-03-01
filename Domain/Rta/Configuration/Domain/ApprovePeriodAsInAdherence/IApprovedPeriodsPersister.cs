using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	public interface IApprovedPeriodsPersister
	{
		void Persist(ApprovedPeriod model);
		void Remove(DateTime until);
	}
}