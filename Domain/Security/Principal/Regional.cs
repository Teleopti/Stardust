using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[Serializable]
	public class Regional : IRegional
	{
		public int CultureLCID { get; set; }
		public int UICultureLCID { get; set; }

		public CultureInfo Culture { get { return (CultureLCID == 0 ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(CultureLCID)).FixPersianCulture(); } }
		public CultureInfo UICulture { get { return (UICultureLCID == 0 ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(UICultureLCID)).FixPersianCulture(); } }

		public TimeZoneInfo TimeZone { get; set; }

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LCID")]
		public Regional(TimeZoneInfo defaultTimeZone, int cultureLCID, int uiCultureLCID)
		{
			TimeZone = defaultTimeZone;
			CultureLCID = cultureLCID;
			UICultureLCID = uiCultureLCID;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public Regional(TimeZoneInfo defaultTimeZone, CultureInfo culture, CultureInfo uiCulture)
        {
            TimeZone = defaultTimeZone;
			CultureLCID = culture.LCID;
			UICultureLCID = uiCulture.LCID;
		}
    }
}
