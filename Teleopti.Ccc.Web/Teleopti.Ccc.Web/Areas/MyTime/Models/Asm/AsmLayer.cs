namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Asm
{
	public class AsmLayer
	{
		public string Payload { get; set; }
		public int RelativeStartInMinutes { get; set; }
		public int LengthInMinutes { get; set; }

		public string Color { get; set; }
	}
}