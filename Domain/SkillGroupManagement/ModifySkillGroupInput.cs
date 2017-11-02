using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class ModifySkillGroupInput
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<SkillInIntradayViewModel> Skills { get; set; }
	}
}