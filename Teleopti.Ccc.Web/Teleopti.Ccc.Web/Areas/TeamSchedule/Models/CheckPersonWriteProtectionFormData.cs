using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class CheckPersonWriteProtectionFormData
	{
		public DateOnly Date;
		public List<Guid> AgentIds;
	}
}