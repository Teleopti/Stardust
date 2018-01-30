namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public interface IApprovedPeriodsPersister
	{
		void Persist(ApprovedPeriodModel model);
	}
}