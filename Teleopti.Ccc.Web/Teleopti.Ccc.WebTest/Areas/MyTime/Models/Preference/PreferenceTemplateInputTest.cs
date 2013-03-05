using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.Preference
{
	[TestFixture]
	public class PreferenceTemplateInputTest{
		[Test]
		public void ShouldNotAcceptEmptyTemplateName()
		{
			var input = new PreferenceTemplateInput {TemplateName = ""};

			var result = input.Validate(null).ToArray();

			result.Count().Should().Be(2);
			result.Last().ErrorMessage.Should().Be(string.Format(Resources.EmptyTemplateName, Resources.ExtendedPreferencesTemplate));
		}
	}
}