using System;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class SkillInIntradayViewModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public bool IsDeleted { get; set; }
		public string SkillType { get; set; }
		public bool DoDisplayData { get; set; }
		public bool IsMultisiteSkill { get; set; }
		public bool ShowAbandonRate { get; set; }
		public bool ShowReforecastedAgents { get; set; }
	}
}