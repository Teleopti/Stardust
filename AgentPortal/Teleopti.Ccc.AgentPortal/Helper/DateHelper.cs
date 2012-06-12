using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    /// <summary>
    /// Date helper class 
    /// </summary>
    public static class DateHelper
	{
		//Nynorsk - Norway 2068
		//Bokmål - Norway 1044
		//Swedish - Sweden 1053
		//German - Germany 1031
		//German - Austria 3079
		//German - Switzerland 2055
		//Danish - Danmark 1030
		//Finnish - Finland 1035
		//France - France 1036
		private static readonly IList<int> Iso8601Cultures = new List<int> { 2068, 1044, 1053, 1031, 3079, 2055, 1030, 1035, 1036 };
		
        public static bool UseIso8601Format(CultureInfo cultureInfo)
        {
        	return Iso8601Cultures.Contains(cultureInfo.LCID);
        }
    }
}