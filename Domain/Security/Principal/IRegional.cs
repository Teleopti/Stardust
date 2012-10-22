using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IRegional
	{
        TimeZoneInfo TimeZone { get; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LCID")]
		int CultureLCID { get; }
		CultureInfo Culture { get; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LCID")]
		int UICultureLCID { get; }
		CultureInfo UICulture { get; }
	}
}