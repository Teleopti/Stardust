using System;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Web.Areas.Gamification.Models
{
	public class ExternalBadgeSettingViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int QualityId { get; set; }
		public bool LargerIsBetter { get; set; }

		public bool Enabled { get; set; }

		public double Threshold { get; set; }
		public double BronzeThreshold { get; set; }
		public double SilverThreshold { get; set; }
		public double GoldThreshold { get; set; }

		public ExternalPerformanceDataType DataType { get; set; }
	}
}