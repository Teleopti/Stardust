using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class LanguageController : ApiController
	{
		[Route("api/Global/Language"), HttpGet]
		public object Language([FromUri]string lang)
		{
			var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);
			var set = UserTexts.Resources.ResourceManager.GetResourceSet(culture, true, true);
			return set.Cast<DictionaryEntry>().ToDictionary(t => t.Key, v => v.Value);
		}
	}
}