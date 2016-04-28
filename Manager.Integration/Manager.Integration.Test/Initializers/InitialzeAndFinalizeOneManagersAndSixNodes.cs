namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagersAndSixNodes : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		public InitialzeAndFinalizeOneManagersAndSixNodes() : base(numberOfNodes: 6,
		                                                           numberOfManagers: 1,
		                                                           useLoadBalancerIfJustOneManager: true,
		                                                           waitToStartUp: true)
		{
		}
	}
}