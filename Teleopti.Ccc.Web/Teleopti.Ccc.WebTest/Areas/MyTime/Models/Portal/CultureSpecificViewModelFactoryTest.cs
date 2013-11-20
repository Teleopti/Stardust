using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using SharpTestsEx;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.Portal
{
	[TestFixture]
	public class SplitButtonSplitterTest
	{

		[Test]
		public void ShoudCreateCutureSpecificViewModel()
		{
			var cultureSpecificViewModelFactory=new CultureSpecificViewModelFactory();
			var cultureSpecificViewModel = cultureSpecificViewModelFactory.CreateCutureSpecificViewModel();
			cultureSpecificViewModel.Rtl.Should().Be.EqualTo(CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft);
			cultureSpecificViewModel.LanguageCode.Should().Be.EqualTo(CultureInfo.CurrentUICulture.IetfLanguageTag);
		}
	}
}
