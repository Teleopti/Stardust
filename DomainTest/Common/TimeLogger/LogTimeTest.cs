using System;
using System.Linq;
using log4net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<LogTimeTester>();
			var log = new LogSpy();
			system.UseTestDouble(log).For<ILog>();
			system.UseTestDouble(new FakeLogManager(log)).For<ILogManager>();
		}

		[Test]
		public void ShouldLog()
		{
			LogTimeTester.TestMethod();
			LogSpy.DebugMessages.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void MessageShouldContainMethodName()
		{
			LogTimeTester.TestMethod();
			var logMessage = LogSpy.DebugMessages.Last();
			logMessage.Should().Contain("LogTimeTester");
			logMessage.Should().Contain("TestMethod");
		}

		[Test]
		public void ShouldHaveCorrectLoggerName()
		{
			LogTimeTester.TestMethod();
			LogSpy.LastLoggerName.Should().Be.EqualTo("Teleopti.TestLog");
		}

		[Test]
		public void ShouldNotLogExceptionTextIfNoException()
		{
			LogTimeTester.TestMethod();
			var logMessage = LogSpy.DebugMessages.Last();
			logMessage.ToLower().Should().Not.Contain("exception");
		}

		[Test]
		public void ShouldLogExceptionTextIfException()
		{
			Assert.Throws<NotImplementedException>(() => LogTimeTester.TestMethodThatThrows());
			var logMessage = LogSpy.DebugMessages.Last();
			logMessage.ToLower().Should().Contain("exception");
		}

		[Test]
		public void ShouldLogMethodNameIfException()
		{
			Assert.Throws<NotImplementedException>(() => LogTimeTester.TestMethodThatThrows());
			var logMessage = LogSpy.DebugMessages.Last();
			logMessage.Should().Contain("LogTimeTester");
			logMessage.Should().Contain("TestMethod");
		}

		[Test]
		public void ShouldHaveLowestOrderOfThemAll()
		{
			var orderOfLogTime = new TestLogAttribute().Order;
			foreach (var attrType in typeof(TestLogAttribute).Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(AspectAttribute)) && t != typeof(TestLogAttribute)))
			{
				var attr = (AspectAttribute)Activator.CreateInstance(attrType);
				if(attr.Order<=orderOfLogTime)
					Assert.Fail(string.Format("Attribute {0} has lower or equal Order than LogTimeAttribute. This is (probably) wrong because we want LogTime to measure the time spent in {0} as well.", attrType.FullName));
			}
		}

	}
}