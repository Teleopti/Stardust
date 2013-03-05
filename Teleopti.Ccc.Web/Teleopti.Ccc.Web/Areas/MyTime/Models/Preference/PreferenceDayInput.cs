using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayInput : PreferenceInput
	{
		public DateOnly Date { get; set; }
		public string TemplateName { get; set; }
	}
}