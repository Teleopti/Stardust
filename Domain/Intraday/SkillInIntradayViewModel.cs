using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillInIntradayViewModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public bool IsDeleted { get; set; }
		public string SkillType { get; set; }
		public bool DoDisplayData { get; set; }
	}
}