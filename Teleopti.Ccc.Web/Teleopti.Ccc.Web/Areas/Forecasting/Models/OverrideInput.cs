namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class OverrideInput : ModifyInput
	{
		public double? OverrideTasks { get; set; }
		public double? OverrideTalkTime { get; set; }
		public double? OverrideAfterCallWork { get; set; }
		public bool ShouldSetOverrideTasks { get; set; }
		public bool ShouldSetOverrideTalkTime { get; set; }
		public bool ShouldSetOverrideAfterCallWork { get; set; }
	}
}