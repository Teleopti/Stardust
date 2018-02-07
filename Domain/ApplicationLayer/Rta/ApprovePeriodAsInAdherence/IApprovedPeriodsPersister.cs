namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence
{
	public interface IApprovedPeriodsPersister
	{
		void Persist(ApprovedPeriod model);
	}
}