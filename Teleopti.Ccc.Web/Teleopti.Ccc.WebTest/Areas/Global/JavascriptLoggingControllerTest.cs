using System;
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
		public void ShouldNotCrashIfAnyDefaultValueIsUsed()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(new FakeLogManagerWrapper(logSpy));

			target.LogError(new JavascriptLog());

			logSpy.ErrorMessages.Single()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldLogMessage()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(new FakeLogManagerWrapper(logSpy));
			var content = new JavascriptLog {Message = RandomName.Make()};

			target.LogError(content);

			logSpy.ErrorMessages.Single()
				.Should().Contain(content.Message);
		}

		[Test]
		public void ShouldLogLineNumber()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(new FakeLogManagerWrapper(logSpy));
			var content = new JavascriptLog { LineNumber = new Random().Next() };

			target.LogError(content);

			logSpy.ErrorMessages.Single()
				.Should().Contain(content.LineNumber.ToString());
		}

		[Test]
		public void ShouldLogParentUrl()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(new FakeLogManagerWrapper(logSpy));
			var content = new JavascriptLog { ParentUrl = RandomName.Make() };

			target.LogError(content);

			logSpy.ErrorMessages.Single()
				.Should().Contain(content.ParentUrl);
		}

		[Test]
		public void ShouldLogUrl()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(new FakeLogManagerWrapper(logSpy));
			var content = new JavascriptLog { Url = RandomName.Make() };

			target.LogError(content);

			logSpy.ErrorMessages.Single()
				.Should().Contain(content.Url);
		}

		[Test]
		public void ShouldLogUserAgent()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(new FakeLogManagerWrapper(logSpy));
			var content = new JavascriptLog { Url = RandomName.Make() };

			target.LogError(content);

			logSpy.ErrorMessages.Single()
				.Should().Contain(content.Url);
		}

		[Test]
		public void ShouldLogToTeleoptiJavascript()
		{
			var logSpy = new LogSpy();
			var target = new JavascriptLoggingController(new FakeLogManagerWrapper(logSpy));

			target.LogError(new JavascriptLog());

			logSpy.LastLoggerName
				.Should().Be.EqualTo("Teleopti.Javascript");
		}
	}
}