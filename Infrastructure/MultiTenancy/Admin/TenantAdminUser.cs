namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class TenantAdminUser
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Email { get; set; }
		public virtual string Password { get; set; }
		public virtual string  AccessToken { get; set; } 
	}
}