using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayInput : PreferenceInput
	{
		public DateOnly Date { get; set; }
		public string TemplateName { get; set; }
	}

    public class MultiPreferenceDaysInput : PreferenceInput
    {
        public IEnumerable<DateOnly> Dates { get; set; }
		public string TemplateName { get; set; }
	}
}
