namespace Manager.Integration.Test.Initializers
{
	public class InitializeAndFinalizeOneManagerAndNodes : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		protected InitializeAndFinalizeOneManagerAndNodes(int numberOfNodes = 1) : base(numberOfNodes,
		                                                         numberOfManagers: 1,
		                                                         useLoadBalancerIfJustOneManager: true,
		                                                         waitToStartUp: true)
		{
		}
		public new int NumberOfNodes => base.NumberOfNodes;
	}
}