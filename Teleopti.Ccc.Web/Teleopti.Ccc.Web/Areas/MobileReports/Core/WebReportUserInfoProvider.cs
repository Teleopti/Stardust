using System.Globalization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	using Models.Domain;

	public class WebReportUserInfoProvider : IWebReportUserInfoProvider
	{
		private readonly IPrincipalProvider _principalProvider;
		private readonly IPersonRepository _personRepository;

		public WebReportUserInfoProvider(IPrincipalProvider principalProvider,
		                                 IPersonRepository personRepository)
		{
			_principalProvider = principalProvider;
			_personRepository = personRepository;
		}

		#region IWebReportUserInfoProvider Members

		public WebReportUserInformation GetUserInformation()
		{
			// TODO: in
			TeleoptiPrincipal teleoptiPrincipal = _principalProvider.Current();
			var teleoptiIdentity = teleoptiPrincipal.Identity as TeleoptiIdentity;

			IPerson person = teleoptiPrincipal.GetPerson(_personRepository);
			IRegional regional = teleoptiPrincipal.Regional;

			return new WebReportUserInformation
			       	{
			       		BusinessUnitCode = teleoptiIdentity.BusinessUnit.Id.Value,
						LanguageId = CultureInfo.CreateSpecificCulture(regional.UICulture.TwoLetterISOLanguageName).LCID,
						//LanguageId = regional.UICulture.LCID,
			       		// Unable to find PersonId in Principal!?
			       		PersonCode = person.Id.Value,
			       		TimeZoneCode = regional.TimeZone.Id
			       	};
		}

		#endregion
	}
}