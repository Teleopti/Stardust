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
		}

		public virtual string Name { get; protected set; }
		public virtual string ApplicationConnectionString { get; protected set; }
		public virtual string AnalyticsConnectionString { get; protected set; }

		public virtual void SetApplicationConnectingString(string applicationConnectionString)
		{

		}

		public virtual void SetAnalyticsConnectionString(string analyticsConnectionString)
		{
		}
	}
}