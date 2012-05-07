using System.Globalization;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/05/")]
	public class Regional : IRegional
    {
		[DataMember]
		public ICccTimeZoneInfo TimeZone { get; set; }
		[DataMember]
		public CultureInfo Culture { get; set; }
		[DataMember]
		public CultureInfo UICulture { get; set; }

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
