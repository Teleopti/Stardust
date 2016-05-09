namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNode : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		public InitialzeAndFinalizeOneManagerAndOneNode() : base(numberOfNodes: 1,
		                                                         numberOfManagers: 1,
		                                                         useLoadBalancerIfJustOneManager: true,
		                                                         waitToStartUp: true)
		{
		}
	}
}