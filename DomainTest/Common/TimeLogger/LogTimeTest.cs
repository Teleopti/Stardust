using System;
using log4net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.DomainTest.Aop.TestDoubles;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Common.TimeLogger
{
	[DomainTest]
	public class LogTimeTest : ISetup
	{
		public LogTimeTester LogTimeTester;
		public LogSpy LogSpy;

		[Test]
		public void ShouldCreateOneLogOutput()
		{
			LogTimeTester.TestMethod();
			LogSpy.DebugMessages.Count
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void MessageShouldContainMethodName()
		{
			LogTimeTester.TestMethod();
			var logMessage = LogSpy.DebugMessages[0];
			logMessage.Should().Contain("LogTimeTester");
			logMessage.Should().Contain("TestMethod");
		}

		[Test]
		public void ShouldHaveCorrectLoggerName()
		{
			LogTimeTester.TestMethod();
			LogSpy.LastLoggerName.Should().Be.EqualTo("Teleopti.LogTime");
		}

		[Test]
		public void ShouldNotLogExceptionTextIfNoException()
		{
			LogTimeTester.TestMethod();
			var logMessage = LogSpy.DebugMessages[0];
			logMessage.ToLower().Should().Not.Contain("exception");
		}

		[Test]
		public void ShouldLogExceptionTextIfException()
		{
			Assert.Throws<NotImplementedException>(() => LogTimeTester.TestMethodThatThrows());
			var logMessage = LogSpy.DebugMessages[0];
			logMessage.ToLower().Should().Contain("exception");
		}

		[Test]
		public void ShouldLogMethodNameIfException()
		{
			Assert.Throws<NotImplementedException>(() => LogTimeTester.TestMethodThatThrows());
			var logMessage = LogSpy.DebugMessages[0];
			logMessage.Should().Contain("LogTimeTester");
			logMessage.Should().Contain("TestMethod");
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<LogTimeTester>();
			var fakeLogger = new LogSpy();
			system.UseTestDouble(fakeLogger).For<ILog>();
			system.UseTestDouble(new FakeLogManagerWrapper(fakeLogger)).For<ILogManagerWrapper>();
		}
	}
}