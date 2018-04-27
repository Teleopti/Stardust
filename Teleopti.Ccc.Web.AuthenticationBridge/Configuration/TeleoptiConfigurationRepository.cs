using System;
using AuthBridge.Configuration;
using AuthBridge.Model;

namespace Teleopti.Ccc.Web.AuthenticationBridge.Configuration
{
	public class TeleoptiConfigurationRepository : IConfigurationRepository
	{
		private readonly IConfigurationRepository inner = new DefaultConfigurationRepository();

		public ClaimProvider RetrieveIssuer(Uri host, Uri identifier)
		{
			return inner.RetrieveIssuer(host, identifier);
		}

		public ClaimProvider[] RetrieveIssuers(Uri host)
		{
			return inner.RetrieveIssuers(host);
		}

		public Scope RetrieveScope(Uri host, Uri identifier)
		{
			return inner.RetrieveScope(host, identifier);
		}

		public ScopeElement RetrieveDefaultScope(Uri host)
		{
			return inner.RetrieveDefaultScope(host);
		}

		public MultiProtocolIssuer MultiProtocolIssuer => inner.MultiProtocolIssuer;
	}
}