namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class Tenant
	{
#pragma warning disable 169
		private int id;
#pragma warning restore 169

		public virtual string Name { get; protected set; }
	}
}