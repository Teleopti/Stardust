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
	}
}