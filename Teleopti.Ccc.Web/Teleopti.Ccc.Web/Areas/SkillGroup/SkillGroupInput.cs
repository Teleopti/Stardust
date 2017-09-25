using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.SkillGroup
{
	public class SkillGroupInput
	{
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}