using SharpTestsEx;
using Teleopti.Ccc.DomainTest.Common;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Repository.Hierarchy;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{

	/// <summary>
	/// Hard to test this one properly...
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-02-28
	/// </remarks>
	[TestFixture]
	public class PerformanceOutputTest
	{
		[Test]
		public void VerifyNormalPerformanceOutput()
		{
			//just check that nothing fails...
			using (PerformanceOutput.ForOperation("for test"))
			{
			}
		}

		[Test]
		public void VerifyNoListener()
		{
			var levelBefore = ((Logger)LogManager.GetLogger(typeof(PerformanceOutput)).Logger).Level;
			try
			{
				((Logger)LogManager.GetLogger(typeof(PerformanceOutput)).Logger).Level = Level.Fatal;

				//just check that nothing fails...
				using (PerformanceOutput.ForOperation("dummy")) { }
			}
			finally
			{
				((Logger)LogManager.GetLogger(typeof(PerformanceOutput)).Logger).Level = levelBefore;
			}
		}

		[Test]
		public void ShouldListenToCorrectName()
		{
			var appender = setUpMemoryAppender("Roger");
			using (PerformanceOutput.ForOperation("Foo", "Roger Kratz")) { }
			appender.GetEvents().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotListenToCorrectName()
		{
			var appender = setUpMemoryAppender("Roger");
			using (PerformanceOutput.ForOperation("Foo", "Peter Westlin")) { }
			appender.GetEvents().Should().Be.Empty();
		}

		private MemoryAppender setUpMemoryAppender(string listeningTo)
		{
			var filter = new LoggerMatchFilter { LoggerToMatch = listeningTo };
			var memAppender = new MemoryAppender();
			memAppender.AddFilter(filter);
			memAppender.AddFilter(new DenyAllFilter());
			BasicConfigurator.Configure(memAppender);
			return memAppender;
		}

		[TearDown]
		public void Teardown()
		{
			BasicConfigurator.Configure(new DoNothingAppender());
		}
	}
}
