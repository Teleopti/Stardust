using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase
{
	public class LayoutBaseViewModel
	{
		public string Title { get; set; }
		public CultureSpecificViewModel CultureSpecific { get; set; }
		public string Footer { get; set; }
		public DatePickerGlobalizationViewModel DatePickerGlobalization { get; set; }
		public DateTime? FixedDate { get; set; }

		public string Version
		{
			get
			{
				var version = typeof (LayoutBaseViewModel).Assembly.GetName().Version;
				return version == new Version(version.Major, 0, 0, 0)
					       ? DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")
					       : version.ToString();
			}
		}
	}
}