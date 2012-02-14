using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class LayoutBaseHtmlHelperTest
	{

		[Test]
		public void ShouldReturnDirClassAttributeWithValueWhenRtl()
		{
			var dirRtl = new MvcHtmlString(" class=\"rtl\"");
			var viewData = new ViewDataDictionary
			               	{
			               		{
			               			"LayoutBase",
			               			new LayoutBaseViewModel
			               				{CultureSpecific = new CultureSpecificViewModel {Rtl = true}}
			               			}
			               	};
			var target = new LayoutBaseHtmlHelper(new TestHtmlHelperBuilder().CreateHtmlHelper(viewData));

			var result = target.FullDirClass();
			result.ToString().Should().Be.EqualTo(dirRtl.ToString());
		}

		[Test]
		public void ShouldReturnLangAttribute()
		{
			var expectedResult = new MvcHtmlString(" lang=\"my-CS\"");
			var viewData = new ViewDataDictionary
			               	{
			               		{
			               			"LayoutBase",
			               			new LayoutBaseViewModel
			               				{CultureSpecific = new CultureSpecificViewModel {LanguageCode = "my-CS"}}
			               			}
			               	};
			var target = new LayoutBaseHtmlHelper(new TestHtmlHelperBuilder().CreateHtmlHelper(viewData));

			var result = target.FullLangAttribute();
			result.ToString().Should().Be.EqualTo(expectedResult.ToString());
		}

		[Test]
		public void ShouldBeEmptyInNonRtl()
		{
			var viewData = new ViewDataDictionary
			               	{
			               		{
			               			"LayoutBase",
			               			new LayoutBaseViewModel
			               				{CultureSpecific = new CultureSpecificViewModel {Rtl = false}}
			               			}
			               	};
			var target = new LayoutBaseHtmlHelper(new TestHtmlHelperBuilder().CreateHtmlHelper(viewData));

			var result = target.FullDirAttribute();
			result.ToString().Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFindDatePickerGlobalizationFromViewData()
		{
			var viewData = new ViewDataDictionary
			               	{
			               		{
			               			"LayoutBase",
			               			new LayoutBaseViewModel
			               				{DatePickerGlobalization = new DatePickerGlobalizationViewModel()}
			               			}
			               	};
			var target = new LayoutBaseHtmlHelper(new TestHtmlHelperBuilder().CreateHtmlHelper(viewData));

			target.DatePickerGlobalizationAsJson();
		}
	}
}