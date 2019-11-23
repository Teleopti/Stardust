namespace Manager.IntegrationTest.Initializers
{
	public class InitializeAndFinalizeSixManagersAndSixNodes : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		public InitializeAndFinalizeSixManagersAndSixNodes() : base(numberOfNodes: 6,
		                                                           numberOfManagers: 6,
		                                                           useLoadBalancerIfJustOneManager: true,
		                                                           waitToStartUp: false)
		{
		}
	}
}