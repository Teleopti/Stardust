namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNodeSetup : InitializeAndFinalizeBaseOnSetup
	{
		public InitialzeAndFinalizeOneManagerAndOneNodeSetup() : base(numberOfNodes: 1,
		                                                         numberOfManagers: 1,
		                                                         useLoadBalancerIfJustOneManager: true,
		                                                         waitToStartUp: false)
		{
		}
	}
}