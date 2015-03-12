namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	//TODO: tenant - another name here? name collision with namespace. Or change namespace name...
	public class Tenant
	{
		//TODO: tenant, when we move to seperate db, we can remove default name here (and in db)
		public const string DefaultName = "Teleopti WFM";

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