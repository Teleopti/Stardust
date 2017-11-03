namespace Teleopti.Ccc.Web.Areas.Gamification.Models
{
	public class QualityInfoViewModel
	{
		public int QualityId { get; set; }
		public string QualityName { get; set; }
		public string QualityType { get; set; }
		public double ScoreWeight { get; set; }

		// Do we need log_object_id and original_id?
	}
}