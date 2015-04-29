using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class LogonInfo
	{
		public Guid PersonId { get; set; }
		public string LogonName { get; set; }
		public string Identity { get; set; }
	}
}