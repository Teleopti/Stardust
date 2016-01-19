using System;
using System.Linq;
using log4net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
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

		[Test]
		public void ShouldHaveLowestOrderOfThemAll()
		{
			var orderOfLogTime = new LogTimeAttribute().Order;
			foreach (var attrType in typeof(LogTimeAttribute).Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(AspectAttribute)) && t != typeof(LogTimeAttribute)))
			{
				var attr = (AspectAttribute)Activator.CreateInstance(attrType);
				if(attr.Order<=orderOfLogTime)
					Assert.Fail(string.Format("Attribute {0} has lower or equal Order than LogTimeAttribute. This is (probably) wrong because we want LogTime to measure the time spent in {0} as well.", attrType.FullName));
			}
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