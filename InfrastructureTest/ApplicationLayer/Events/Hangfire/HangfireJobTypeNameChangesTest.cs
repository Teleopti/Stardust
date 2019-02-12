using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[RealHangfireTest]
	public class HangfireJobTypeNameChangesTest : IExtendSystem, ITestInterceptor
	{
		public Lazy<HangfireUtilities> Hangfire;
		public IEventPublisher Publisher;
		public TestHandler Handler;
		public WithAnalyticsUnitOfWork Analytics;
		public PersistedTypeMapperForTest Mapper;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
		}

		public void OnBefore()
		{
			Mapper.DynamicMappingsForTestProjects = false;
			Mapper.StaticMappingsForTestProjects = true;
		}

		[Test]
		public void ShouldHandleMovedEvent()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			replaceInJobs("HangfireJobTypeNameChangesTest+TestEvent", "Teleopti.Original.Assembly.Namespace.MovedEventName, Teleopti.Original");
			assumeJobContains("Teleopti.Original.Assembly.Namespace.MovedEventName, Teleopti.Original");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMovedHangfireEventJobType()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			replaceInJobs("Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared", "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Ccc.Infrastructure");
			assumeJobContains("Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Ccc.Infrastructure");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMovedHangfireEventJobTypeFromDifferentAssemblyVersions()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			replaceInJobs("Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null");
			assumeJobContains("Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMovedHangfireServerProcessor()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			replaceInJobs("Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Ccc.Infrastructure", "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Wfm.Shared");
			assumeJobContains("Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Wfm.Shared");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}
		
		public class TestEvent : IEvent
		{
			public string Data { get; set; }
		}

		public class TestHandler :
			IHandleEvent<TestEvent>,
			IRunOnHangfire
		{
			public string Got;

			public void Handle(TestEvent @event)
			{
				Got = @event.Data;
			}
		}

		private void replaceInJobs(string replace, string with)
		{
			Analytics.Do(x =>
			{
				x.Current().FetchSession()
					.CreateSQLQuery($@"
UPDATE Hangfire.Job 
SET 
	InvocationData = REPLACE(InvocationData, '{replace}', '{with}'),
	Arguments = REPLACE(Arguments, '{replace}', '{with}')
")
					.ExecuteUpdate();
			});
		}

		private void assumeJobContains(string contains)
		{
			Analytics.Do(x =>
			{
				x.Current().FetchSession()
					.CreateSQLQuery("SELECT InvocationData, Arguments FROM Hangfire.Job")
					.List<object[]>()
					.ForEach(row =>
					{
						var data = row.StringJoin(o => o as string);
						Assert.That(data, Does.Contain(contains));
					});
			});
		}

		private void outputJobData()
		{
			Analytics.Do(x =>
			{
				x.Current().FetchSession()
					.CreateSQLQuery("SELECT InvocationData, Arguments FROM Hangfire.Job")
					.List<object[]>()
					.ForEach(row =>
					{
						row.ForEach(data =>
						{
							Console.WriteLine();
							Console.WriteLine(data);
						});
					});
			});
		}
	}
}