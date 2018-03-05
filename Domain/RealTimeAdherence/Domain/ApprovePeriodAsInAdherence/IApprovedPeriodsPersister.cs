using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	[RemoveMeWithToggle(Toggles.RTA_RemoveApprovedOOA_47721)]
	public interface IApprovedPeriodsPersister
	{
		void Persist(ApprovedPeriod model);
		void Remove(DateTime until);
	}
}