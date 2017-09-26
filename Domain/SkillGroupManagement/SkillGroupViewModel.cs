using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class SkillGroupViewModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public IEnumerable<SkillInIntradayViewModel> Skills { get; set; }
	}
}