namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public interface IApprovedPeriodsPersister
	{
		void Persist(ApprovedPeriod model);
	}
}