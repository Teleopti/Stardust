using System;
using Newtonsoft.Json;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class TenantAuthenticationData
	{
		public string Tenant { get; set; }
		public string ApplicationLogonName { get; set; }
		public string Password { get; set; }
		public string Identity { get; set; }
		[JsonIgnore]
		public bool Changed { get; set; }
		public Guid PersonId { get; set; }
	}
}