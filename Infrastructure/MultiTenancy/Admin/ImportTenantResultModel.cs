using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class ImportTenantResultModel
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int TenantId { get; set; }
	}
}