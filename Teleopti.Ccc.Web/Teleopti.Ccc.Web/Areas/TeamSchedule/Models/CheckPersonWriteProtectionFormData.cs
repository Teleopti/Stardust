using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class CheckPersonWriteProtectionFormData
	{
		public DateOnly Date;
		public List<Guid> AgentIds;
	}
}