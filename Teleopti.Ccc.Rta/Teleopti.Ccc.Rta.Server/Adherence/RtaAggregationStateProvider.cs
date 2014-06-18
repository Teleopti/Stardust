namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class RtaAggregationStateProvider
	{
		private readonly RtaAggregationState aggregationState;

		public RtaAggregationStateProvider(IOrganizationForPerson organizationForPerson)
		{
			aggregationState = new RtaAggregationState(organizationForPerson);
		}

		public RtaAggregationState GetState()
		{
			return aggregationState;
		}
	}
}