using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillAreaViewModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public IEnumerable<SkillInIntradayViewModel> Skills { get; set; }
	}
}