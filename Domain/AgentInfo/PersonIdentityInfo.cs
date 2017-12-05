using System;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PersonIdentityInfo
	{
		public Guid PersonId { get; set; }
		public string AppLogonName { get; set; }
		public string EmployeeNumber { get; set; }
	}
}