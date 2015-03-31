using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.Http;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class LanguageController : ApiController
	{
		[Route("api/Global/Language"), HttpGet, Authorize]
		public object Language([FromUri]string lang)
		{
			var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);
			var set = UserTexts.Resources.ResourceManager.GetResourceSet(culture, false, true);
			return set.Cast<DictionaryEntry>().ToDictionary(t => t.Key, v => v.Value);
		}
	}
}