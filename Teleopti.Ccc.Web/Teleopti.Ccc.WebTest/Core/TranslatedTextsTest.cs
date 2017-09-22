using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Core
{
	public class TranslatedTextsTest
	{
		[Test]
		public void ShouldGetTextsForEnglish()
		{
			var target = new TranslatedTexts();
			target.For(CultureInfo.GetCultureInfo("en"))
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ExoticLanguageShouldHaveNoNulls()
		{
			var target = new TranslatedTexts();
			foreach (var text in target.For(CultureInfo.GetCultureInfo(1109)))
			{
				text.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ExoticLanguageShouldHaveAsManyEntriesAsEnglish()
		{
			var target = new TranslatedTexts();
			target.For(CultureInfo.GetCultureInfo(1133)).Count
				.Should().Be.EqualTo(target.For(CultureInfo.GetCultureInfo("en")).Count);
		}
	}
}