using System;

namespace Teleopti.Ccc.Domain.Rta.ApprovePeriodAsInAdherence
{
	public interface IApprovedPeriodsPersister
	{
		void Persist(ApprovedPeriod model);
		void Remove(DateTime until);
	}
}