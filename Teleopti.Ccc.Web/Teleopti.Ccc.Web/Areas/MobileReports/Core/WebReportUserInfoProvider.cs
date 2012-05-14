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
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IPersonRepository _personRepository;

		public WebReportUserInfoProvider(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
		                                 IPersonRepository personRepository)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_personRepository = personRepository;
		}

		#region IWebReportUserInfoProvider Members

		public WebReportUserInformation GetUserInformation()
		{
			var teleoptiPrincipal = _currentTeleoptiPrincipal.Current();
			var teleoptiIdentity = teleoptiPrincipal.Identity as ITeleoptiIdentity;

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