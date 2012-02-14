using Teleopti.Ccc.Sdk.Client;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class TestContext
	{
		public void Make(string serviceUrl)
		{
			Session = new Session();
			Client = new SdkServiceClient(Session, serviceUrl);
		}

		public void Clear()
		{
			Session = null;
			Client.Dispose();
		}

		public SdkServiceClient Client { get; private set; }
		public Session Session { get; private set; }
	}
}