namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeSixManagersAndSixNodes : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeSixManagersAndSixNodes() : base(numberOfNodes: 6,
		                                                           numberOfManagers: 6,
		                                                           useLoadBalancerIfJustOneManager: true,
		                                                           waitToStartUp: false)
		{
		}
	}
}