using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillStatusModel
	{
		public string	SkillName { get; set; }
		public int  Severity { get; set; }

		public List<SkillStatusMeasure> Measures { get; set; }
	}
}