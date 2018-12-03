using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportHeaderModel
	{
		public DateOnlyPeriod Period { get; set; }
		public string SkillName { get; set; }
		public string SkillTimeZoneName { get; set; }
		public Percent? ServiceLevelPercent { get; set; }
		public double? ServiceLevelSeconds { get; set; }
		public Percent? ShrinkagePercent { get; set; }
		public string Scenario { get; set; }
	}
}