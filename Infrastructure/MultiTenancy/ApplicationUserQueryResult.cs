using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQueryResult
	{
		public Guid PersonId { get; set; }
		public bool Success { get; set; }
		public string Tennant { get; set; }
		public string Password { get; set; }
		public DateTime LastPasswordChange { get; set; }
		public DateTime InvalidAttemptsSequenceStart { get; set; }
		public int InvalidAttempts { get; set; }
		public bool IsLocked { get; set; }
	}
}