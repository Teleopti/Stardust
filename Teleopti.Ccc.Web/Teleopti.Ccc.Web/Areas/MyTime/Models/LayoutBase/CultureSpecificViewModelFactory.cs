using System.Globalization;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase
{
	public class CultureSpecificViewModelFactory : ICultureSpecificViewModelFactory
	{
		public CultureSpecificViewModel CreateCutureSpecificViewModel()
		{
			var currentUiCulture = CultureInfo.CurrentUICulture;
			var isRightToLeft = currentUiCulture.TextInfo.IsRightToLeft;
			var ietfLanguageTag = currentUiCulture.IetfLanguageTag;

			return new CultureSpecificViewModel { LanguageCode = ietfLanguageTag, Rtl = isRightToLeft };
		}
	}
}