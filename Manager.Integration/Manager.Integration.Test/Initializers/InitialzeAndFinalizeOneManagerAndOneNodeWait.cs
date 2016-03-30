namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNodeWait : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		public InitialzeAndFinalizeOneManagerAndOneNodeWait() : base(numberOfNodes: 1,
		                                                         numberOfManagers: 1,
		                                                         useLoadBalancerIfJustOneManager: true,
		                                                         waitToStartUp: true)
		{
		}
	}
}