using System.Collections.Generic;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class LanguageController : ApiController
	{
		private readonly TranslatedTexts _translatedTexts;

		public LanguageController(TranslatedTexts translatedTexts)
		{
			_translatedTexts = translatedTexts;
		}

		[Route("api/Global/Language"), HttpGet]
		public IDictionary<string, string> Language([FromUri]string lang)
		{
			var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);
			return _translatedTexts.For(culture);
		}
	}
}