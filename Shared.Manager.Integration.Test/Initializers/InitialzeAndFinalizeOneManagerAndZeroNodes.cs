﻿namespace Manager.IntegrationTest.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndZeroNodes : InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		public InitialzeAndFinalizeOneManagerAndZeroNodes() : base(numberOfNodes: 0,
		                                                           numberOfManagers: 1,
		                                                           useLoadBalancerIfJustOneManager: true,
		                                                           waitToStartUp: false)
		{
		}
	}
}