using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PersonExternalLogonInfo
	{
		public Guid PersonId { get; set; }
		public List<string> ExternalLogonName { get; set; }
	}
}