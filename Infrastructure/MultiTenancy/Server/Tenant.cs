using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class Tenant
	{
#pragma warning disable 169
		private int id;
#pragma warning restore 169

		protected Tenant(){}

		public Tenant(string tenantName)
		{
			Name = tenantName;
			DataSourceConfiguration = new DataSourceConfiguration();
		}

		public virtual DataSourceConfiguration DataSourceConfiguration { get; protected set; }
		public virtual string Name { get; protected set; }
	}
}