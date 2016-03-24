namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndZeroNodes : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeOneManagerAndZeroNodes() : base(numberOfNodes: 0,
		                                                           numberOfManagers: 1,
		                                                           useLoadBalancerIfJustOneManager: true,
		                                                           waitToStartUp: false)
		{
		}
	}
}