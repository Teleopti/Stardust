using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FilterModel
	{
		public const string ContractFilterType = "contract";
		public const string SiteFilterType = "site";
		public const string TeamFilterType = "team";
		public const string SkillFilterType = "skill";
		
		public Guid Id { get; set; }
		public string FilterType { get; set; }
		public string Name { get; set; }
	}
}