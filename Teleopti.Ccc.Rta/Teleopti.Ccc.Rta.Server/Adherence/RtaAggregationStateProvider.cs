namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class RtaAggregationStateProvider
	{
		private readonly RtaAggregationState snarret;

		public RtaAggregationStateProvider(IOrganizationForPerson organizationForPerson)
		{
			snarret = new RtaAggregationState(organizationForPerson);
		}

		public RtaAggregationState GetState()
		{
			return snarret;
		}
	}
}