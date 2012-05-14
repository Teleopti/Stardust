using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[Serializable]
	public class Regional : IRegional
	{
		private int _cultureId;
		private int _uiCultureId;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CultureInfo Culture { get { return CultureInfo.GetCultureInfo(_cultureId); } set { _cultureId = value == null ? 0 : value.LCID; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CultureInfo UICulture { get { return CultureInfo.GetCultureInfo(_uiCultureId); } set { _uiCultureId = value == null ? 0 : value.LCID; } }

		public ICccTimeZoneInfo TimeZone { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static IRegional FromPerson(IPerson person)
		{
			return new Regional(person.PermissionInformation.DefaultTimeZone(),
			                    person.PermissionInformation.Culture(),
			                    person.PermissionInformation.UICulture());
		}

        public Regional(ICccTimeZoneInfo defaultTimeZone, CultureInfo culture, CultureInfo uiCulture)
        {
            TimeZone = defaultTimeZone;
            Culture = culture;
            UICulture = uiCulture;
        }
    }
}
