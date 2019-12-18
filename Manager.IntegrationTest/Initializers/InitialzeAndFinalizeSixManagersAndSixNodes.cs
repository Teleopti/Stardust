namespace Manager.IntegrationTest.Initializers
{
	public class InitialzeAndFinalizeSixManagersAndSixNodes : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		public InitialzeAndFinalizeSixManagersAndSixNodes() : base(numberOfNodes: 6,
		                                                           numberOfManagers: 6,
		                                                           useLoadBalancerIfJustOneManager: true,
		                                                           waitToStartUp: false)
		{
		}
	}
}