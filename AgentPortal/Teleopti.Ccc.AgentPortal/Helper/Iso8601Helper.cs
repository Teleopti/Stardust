using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    /// <summary>
    /// Date helper class 
    /// </summary>
    public static class Iso8601Helper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static bool UseIso8601Format(CultureInfo cultureInfo)
        {
        	return DateHelper.Iso8601Cultures.Contains(cultureInfo.LCID);
        }
    }
}