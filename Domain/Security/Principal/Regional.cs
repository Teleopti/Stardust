using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[Serializable]
	public class Regional : IRegional
	{
		public Regional(TimeZoneInfo timeZone, CultureInfo culture, CultureInfo uiCulture) :
			this(timeZone, culture?.LCID ?? 0, uiCulture?.LCID ?? 0)
		{
		}

		public Regional(TimeZoneInfo timeZone, int cultureLCID, int uiCultureLCID)
		{
			TimeZone = timeZone;
			CultureLCID = cultureLCID;
			UICultureLCID = uiCultureLCID;
		}

		public TimeZoneInfo TimeZone { get; }
		public int CultureLCID { get; set; }
		public CultureInfo Culture => (CultureLCID == 0 ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(CultureLCID)).FixPersianCulture(ForceUseGregorianCalendar);
		public int UICultureLCID { get; set; }
		public CultureInfo UICulture => (UICultureLCID == 0 ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(UICultureLCID)).FixPersianCulture(ForceUseGregorianCalendar);
		public bool ForceUseGregorianCalendar { get; set; }
	}
}