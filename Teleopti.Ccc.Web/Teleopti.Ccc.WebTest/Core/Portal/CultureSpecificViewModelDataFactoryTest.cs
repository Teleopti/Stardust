using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class CultureSpecificViewModelDataFactoryTest
	{
		private ICultureSpecificViewModelFactory target;

		[SetUp]
		public void Setup()
		{
			target = new CultureSpecificViewModelFactory();
		}

		[Test, SetCulture("en-US"), SetUICulture("en-US")]
		public void ShouldCreateModelWithFromEnUsCulture()
		{
			var result = target.CreateCutureSpecificViewModel();

			result.LanguageCode.Should().Be.EqualTo("en");
			result.Rtl.Should().Be.False();
		}

		[Test, SetCulture("ar-SA"), SetUICulture("ar-SA")]
		public void ShouldCreateModelWithFromArSACulture()
		{
			var result = target.CreateCutureSpecificViewModel();

			result.LanguageCode.Should().Be.EqualTo("ar");
			result.Rtl.Should().Be.True();
		}
	}
}