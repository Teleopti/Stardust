namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class TenantAuditAction
	{
		public PersonInfo PersonInfo { get; set; }
		public bool ThrowOnError = true;
	}

	public class IdentityChangeActionObj : TenantAuditAction
	{

	}

	public class AppLogonChangeActionObj : TenantAuditAction
	{

	}

	public class GenericPersistApiCallActionObj : TenantAuditAction
	{

	}
}
