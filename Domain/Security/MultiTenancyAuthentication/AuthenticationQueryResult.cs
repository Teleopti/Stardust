using System;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class AuthenticationQueryResult
	{
		public bool Success { get; set; }
		public Guid PersonId { get; set; }
		public string Tennant { get; set; }
	}
}