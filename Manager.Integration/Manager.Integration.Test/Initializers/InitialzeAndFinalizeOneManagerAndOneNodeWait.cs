namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNodeWait : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeOneManagerAndOneNodeWait() : base(numberOfNodes: 1,
		                                                         numberOfManagers: 1,
		                                                         useLoadBalancerIfJustOneManager: true,
		                                                         waitToStartUp: true)
		{
		}
	}
}