using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class ModifySkillGroupInput
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}