using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[Serializable]
	public class Regional : IRegional
	{
		public int CultureLCID { get; set; }
		public int UICultureLCID { get; set; }

		public CultureInfo Culture { get { return CultureInfo.GetCultureInfo(CultureLCID); } }
		public CultureInfo UICulture { get { return CultureInfo.GetCultureInfo(UICultureLCID); } }

		public ICccTimeZoneInfo TimeZone { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static IRegional FromPerson(IPerson person)
		{
			var info = person.PermissionInformation;
			var cultureLCID = info.CultureLCID() ?? 0;
			var uiCultureLCID = info.UICultureLCID() ?? 0;
			return new Regional(person.PermissionInformation.DefaultTimeZone(),
			                    cultureLCID,
			                    uiCultureLCID);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static IRegional FromPersonWithThreadCultureFallback(IPerson person)
		{
			var info = person.PermissionInformation;
			var cultureLCID = info.CultureLCID() ?? System.Threading.Thread.CurrentThread.CurrentCulture.LCID;
			var uiCultureLCID = info.UICultureLCID() ?? System.Threading.Thread.CurrentThread.CurrentUICulture.LCID;
			return new Regional(person.PermissionInformation.DefaultTimeZone(),
								cultureLCID,
								uiCultureLCID);
		}

		public Regional(ICccTimeZoneInfo defaultTimeZone, int cultureLCID, int uiCultureLCID)
		{
			TimeZone = defaultTimeZone;
			CultureLCID = cultureLCID;
			UICultureLCID = uiCultureLCID;
		}

        public Regional(ICccTimeZoneInfo defaultTimeZone, CultureInfo culture, CultureInfo uiCulture)
        {
            TimeZone = defaultTimeZone;
			CultureLCID = culture.LCID;
			UICultureLCID = uiCulture.LCID;
		}
    }
}
