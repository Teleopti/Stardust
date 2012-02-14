using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public static class StudentAvailabilityRestrictionExtensions
	{
		public static string FormatLimitationTimes(this IStudentAvailabilityRestriction restriction)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", restriction.StartTimeLimitation.StartTimeString, restriction.EndTimeLimitation.EndTimeString);
		}
	}
}