using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PersonExternalLogonInfo
	{
		public Guid PersonId { get; set; }
		public List<string> ExternalLogonName { get; set; }
	}
}