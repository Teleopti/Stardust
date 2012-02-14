using System.Globalization;
using Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase
{
	public class CultureSpecificViewModelFactory : ICultureSpecificViewModelFactory
	{
		public CultureSpecificViewModel CreateCutureSpecificViewModel()
		{
			var currentUiCulture = CultureInfo.CurrentUICulture;
			var isRightToLeft = currentUiCulture.TextInfo.IsRightToLeft;
			var twoLetterIsoLanguageName = currentUiCulture.TwoLetterISOLanguageName;

			return new CultureSpecificViewModel { LanguageCode = twoLetterIsoLanguageName, Rtl = isRightToLeft };
		}
	}
}