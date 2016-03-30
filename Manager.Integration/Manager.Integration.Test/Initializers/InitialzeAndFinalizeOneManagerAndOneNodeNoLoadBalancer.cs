namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNodeNoLoadBalancer : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		public InitialzeAndFinalizeOneManagerAndOneNodeNoLoadBalancer() : base(numberOfNodes: 1,
		                                                                       numberOfManagers: 1,
		                                                                       useLoadBalancerIfJustOneManager: false,
		                                                                       waitToStartUp: false)
		{
		}
	}
}