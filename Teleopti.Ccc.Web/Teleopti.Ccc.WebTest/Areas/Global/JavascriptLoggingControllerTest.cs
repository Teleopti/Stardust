using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class JavascriptLoggingControllerTest
	{
		[Test]
		public void ShouldLogError()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(logSpy);
			var content = RandomName.Make();

			target.LogError(content);

			logSpy.ErrorMessages.Single()
				.Should().Be.EqualTo(content);
		}
	}
}