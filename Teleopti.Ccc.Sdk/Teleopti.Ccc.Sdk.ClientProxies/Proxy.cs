using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ClientProxies
{
    public class Proxy : ClientBase<ITeleoptiCccSdkInternal>
    {
		public Proxy() { }

		public Proxy(string serviceEndpoint) : base(serviceEndpoint) { }

		public static Proxy GetProxy(string serviceEndpoint)
		{
			return string.IsNullOrEmpty(serviceEndpoint)
					   ? new Proxy()
					   : new Proxy(serviceEndpoint);
		}

        public string GetPasswordPolicy()
        {
            return Channel.GetPasswordPolicy();
        }
    }
}