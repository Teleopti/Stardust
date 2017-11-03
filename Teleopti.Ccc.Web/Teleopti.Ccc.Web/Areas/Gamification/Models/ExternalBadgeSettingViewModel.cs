using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Gamification.Models
{
	public class ExternalBadgeSettingViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool IsBuildIn { get; set; }
		public int QualityId { get; set; }
		public bool LargerIsBetter { get; set; }

		public bool Enabled { get; set; }

		public int Threshold { get; set; }
		public int BronzeThreshold { get; set; }
		public int SilverThreshold { get; set; }
		public int GoldThreshold { get; set; }

		public BadgeUnitType UnitType { get; set; }
	}
}