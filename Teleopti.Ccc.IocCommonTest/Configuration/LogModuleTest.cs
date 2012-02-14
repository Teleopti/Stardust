using System;
using System.Collections.Generic;
using Autofac;
using log4net;
using log4net.Appender;
using log4net.Core;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class LogModuleTest
	{
		private ContainerBuilder containerBuilder;
		private testAppender appender;

		[SetUp]
		public void Setup()
		{
			appender = new testAppender();
			configureLog4Net();
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new LogModule());
			containerBuilder.RegisterType<DummyService>().As<IDummyService>();
			containerBuilder.RegisterType<DummyService2>().As<IDummyService2>();
		}

		[Test]
		public void LogMessageShouldBeWritten()
		{
			using (var container = containerBuilder.Build())
			{
				var svc = container.Resolve<IDummyService>();
				appender.Messages.Should().Be.Empty();
				svc.DoIt();
				appender.Messages.Should().Have.Count.EqualTo(1);
				appender.Messages[0].RenderedMessage.Should().Be.EqualTo("logtest");
			}
		}

		[Test]
		public void InjectedLoggersShouldHaveTheirOwnManager()
		{
			using (var container = containerBuilder.Build())
			{
				var svc = container.Resolve<IDummyService>();
				var svc2 = container.Resolve<IDummyService2>();
				svc.DoIt();
				svc2.DoIt();
				appender.Messages[0].LoggerName.Should().Be.EqualTo(svc.GetType().FullName);
				appender.Messages[1].LoggerName.Should().Be.EqualTo(svc2.GetType().FullName);
			}
		}

		private void configureLog4Net()
		{
			log4net.Config.BasicConfigurator.Configure(appender);
		}

		private class testAppender : IAppender
		{
			public IList<LoggingEvent> Messages { get; private set; }

			public testAppender()
			{
				Messages = new List<LoggingEvent>();
			}

			public void Close()
			{
			}

			public void DoAppend(LoggingEvent loggingEvent)
			{
				Messages.Add(loggingEvent);
			}

			public string Name
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}
		}
	}

	internal class DummyService2 : IDummyService2
	{
		private readonly ILog _log;

		public DummyService2(ILog log)
		{
			_log = log;
		}

		public void DoIt()
		{
			_log.Debug("logtest");
		}
	}

	internal interface IDummyService2
	{
		void DoIt();
	}
	
	internal class DummyService : IDummyService
	{
		private readonly ILog _log;

		public DummyService(ILog log)
		{
			_log = log;
		}

		public void DoIt()
		{
			_log.Debug("logtest");
		}
	}

	internal interface IDummyService
	{
		void DoIt();
	}
}