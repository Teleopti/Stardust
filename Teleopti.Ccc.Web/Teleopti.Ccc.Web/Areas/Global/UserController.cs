using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class UserController : ApiController
	{
		private readonly ICurrentIdentity _currentIdentity;

		public UserController(ICurrentIdentity currentIdentity)
		{
			_currentIdentity = currentIdentity;
		}

		[Route("api/Global/User/CurrentUser"), HttpGet, Authorize]
		public object CurrentUser()
		{
			return new { UserName = _currentIdentity.Current().Name, Language = CultureInfo.CurrentUICulture.IetfLanguageTag };
		}
	}

	public class LanguageController : ApiController
	{
		private readonly ICurrentIdentity _currentIdentity;

		public LanguageController(ICurrentIdentity currentIdentity)
		{
			_currentIdentity = currentIdentity;
		}

		[Route("api/Global/Language"), HttpGet, Authorize]
		public object Language([FromUri]string lang)
		{
			var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);
			var set = UserTexts.Resources.ResourceManager.GetResourceSet(culture, false, true);
			return set.Cast<DictionaryEntry>().ToDictionary(t => t.Key, v => v.Value);
		}
	}
}