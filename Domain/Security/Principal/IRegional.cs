using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IRegional
	{
		TimeZoneInfo TimeZone { get; }
		int CultureLCID { get; }
		CultureInfo Culture { get; }
		int UICultureLCID { get; }
		CultureInfo UICulture { get; }
		bool ForceUseGregorianCalendar { get; set; }
	}
}