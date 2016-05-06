using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.IntegrationTest.Console.Host.Helpers;

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
		public HttpSender HttpSender { get; set; }
		public ManagerUriBuilder MangerUriBuilder { get; set; }
		public HttpRequestManager HttpRequestManager { get; set; }

		public override void SetUp()
		{
			DatabaseHelper.TruncateJobQueueTable(ManagerDbConnectionString);
			DatabaseHelper.TruncateJobTable(ManagerDbConnectionString);
			DatabaseHelper.TruncateJobDetailTable(ManagerDbConnectionString);

			HttpSender = new HttpSender();
			MangerUriBuilder = new ManagerUriBuilder();
			HttpRequestManager = new HttpRequestManager();
		}
	}
}