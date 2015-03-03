using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
	public class TenantAuthenticationData
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Identity { get; set; }
		public DateOnly? TerminalDate { get; set; }
		public bool Changed { get; set; }
	}
}