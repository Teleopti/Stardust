using System.Globalization;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/05/")]
	public class Regional : IRegional
	{

		[DataMember]
		private string _cultureName;
		private CultureInfo _culture;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CultureInfo Culture
		{
			get { return _culture ?? (_culture = CultureInfo.GetCultureInfo(_cultureName)); }
			set
			{
				_culture = value;
				_cultureName = _culture.Name;
			}
		}

		[DataMember]
		private string _uiCultureName;
		private CultureInfo _uiCulture;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CultureInfo UICulture
		{
			get { return _uiCulture ?? (_uiCulture = CultureInfo.GetCultureInfo(_uiCultureName)); }
			set
			{
				_uiCulture = value;
				_uiCultureName = _uiCulture.Name;
			}
		}

		[DataMember]
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
