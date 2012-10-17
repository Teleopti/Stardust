using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IRegional
	{
		ICccTimeZoneInfo TimeZone { get; }
		int CultureLCID { get; }
		CultureInfo Culture { get; }
		int UICultureLCID { get; }
		CultureInfo UICulture { get; }
	}
}