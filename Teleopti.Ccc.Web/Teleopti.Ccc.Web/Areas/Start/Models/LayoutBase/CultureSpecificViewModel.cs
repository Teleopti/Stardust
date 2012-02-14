namespace Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase
{
	public class CultureSpecificViewModel
	{
		public bool Rtl { get; set; }

		/// <summary>
		/// Should be ISO 639-1 Language Codes
		/// </summary>
		public string LanguageCode { get; set; }
	}
}