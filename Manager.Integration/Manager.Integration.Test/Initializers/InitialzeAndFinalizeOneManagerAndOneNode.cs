namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNode : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeOneManagerAndOneNode() : base(numberOfNodes: 1,
		                                                         numberOfManagers: 1,
		                                                         useLoadBalancerIfJustOneManager: true,
		                                                         waitToStartUp: false)
		{
		}
	}
}