using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
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
		public IJsonEventDeserializer Deserializer;
		public IJsonEventSerializer Serializer;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
		}

		public void OnBefore()
		{
			Mapper.DynamicMappingsForTestProjects = false;
			Mapper.StaticMappingsForTestProjects = true;
			Hangfire.Value.CleanQueue();
		}

		[Test]
		public void ShouldHandleMovedEvent()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			makeJobLegacyWith(v => v.EventType = "Teleopti.Original.Assembly.Namespace.MovedEventName, Teleopti.Original");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMovedHangfireEventJobType()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			makeJobLegacyWith(v => v.HangfireEventJobType = "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Ccc.Infrastructure");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMovedHangfireEventJobTypeFromDifferentAssemblyVersions()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			makeJobLegacyWith(v => v.HangfireEventJobType = "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMappedHangfireEventJobType()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			makeJobLegacyWith(v => v.HangfireEventJobType = Mapper.NameForPersistence(typeof(HangfireEventJob)));

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMovedHangfireEventServer()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			makeJobLegacyWith(v => v.HangfireEventServer = "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Wfm.Shared");

			Hangfire.Value.EmulateWorkerIteration();

			Hangfire.Value.ThrowExceptionFromAnyFailedJob();
			Handler.Got.Should().Be("data");
		}

		[Test]
		public void ShouldHandleMappedHangfireEventServer()
		{
			Publisher.Publish(new TestEvent {Data = "data"});
			makeJobLegacyWith(v => v.HangfireEventServer = Mapper.NameForPersistence(typeof(HangfireEventServer)));

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

		private void makeJobLegacyWith(Action<JobValues> mutation)
		{
			var values = new JobValues();
			mutation.Invoke(values);
			var mutated = source
				.Replace("[HangfireEventServer]", values.HangfireEventServer)
				.Replace("[HangfireEventJobType]", values.HangfireEventJobType)
				.Replace("[EventType]", values.EventType);
			var invocationData = Deserializer.DeserializeEvent<InvocationDataArguments>(mutated);
			Analytics.Do(x => x.Current().FetchSession()
				.CreateSQLQuery($@"UPDATE Hangfire.Job SET InvocationData = '{mutated}', Arguments = '{invocationData.Arguments}'")
				.ExecuteUpdate());

			outputJobData();
		}

/*
{
	"Type": "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Ccc.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Method": "Process",
	"ParameterTypes": "[\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"]",
	"Arguments": "[\"\\\"HangfireJobTypeNameChangesTest+TestHandler got TestEvent on TestData\\\"\",\"{\\\"DisplayName\\\":\\\"HangfireJobTypeNameChangesTest+TestHandler got TestEvent on TestData\\\",\\\"Tenant\\\":\\\"TestData\\\",\\\"Attempts\\\":3,\\\"AllowFailures\\\":0,\\\"Event\\\":{\\\"$type\\\":\\\"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireJobTypeNameChangesTest+TestEvent, Teleopti.Ccc.InfrastructureTest\\\",\\\"Data\\\":\\\"data\\\"},\\\"HandlerTypeName\\\":\\\"HangfireJobTypeNameChangesTest+TestHandler\\\",\\\"RunInterval\\\":1}\"]"
}
*/

		// BuildMyString.com generated code. Please enjoy your string responsibly.
		const string source = "{" +
							  "\"Type\": \"[HangfireEventServer]\"," +
							  "\"Method\": \"Process\"," +
							  "\"ParameterTypes\": \"[\\\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\\\",\\\"[HangfireEventJobType]\\\"]\"," +
							  "\"Arguments\": \"[\\\"\\\\\\\"HangfireJobTypeNameChangesTest+TestHandler got TestEvent on TestData\\\\\\\"\\\",\\\"{\\\\\\\"DisplayName\\\\\\\":\\\\\\\"HangfireJobTypeNameChangesTest+TestHandler got TestEvent on TestData\\\\\\\",\\\\\\\"Tenant\\\\\\\":\\\\\\\"TestData\\\\\\\",\\\\\\\"Attempts\\\\\\\":3,\\\\\\\"AllowFailures\\\\\\\":0,\\\\\\\"Event\\\\\\\":{\\\\\\\"$type\\\\\\\":\\\\\\\"[EventType]\\\\\\\",\\\\\\\"Data\\\\\\\":\\\\\\\"data\\\\\\\"},\\\\\\\"HandlerTypeName\\\\\\\":\\\\\\\"HangfireJobTypeNameChangesTest+TestHandler\\\\\\\",\\\\\\\"RunInterval\\\\\\\":1}\\\"]\"" +
							  "}";

		private class JobValues
		{
			public string HangfireEventServer { get; set; } = "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Ccc.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			public string HangfireEventJobType { get; set; } = "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			public string EventType { get; set; } = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireJobTypeNameChangesTest+TestEvent, Teleopti.Ccc.InfrastructureTest";
		}

		public class InvocationDataArguments
		{
			public string Arguments { get; set; }
		}

		private void outputJobData()
		{
#if DEBUG

			Console.WriteLine("Job looks like this:");

			Analytics.Do(x =>
			{
				x.Current().FetchSession()
					.CreateSQLQuery("SELECT InvocationData, Arguments FROM Hangfire.Job")
					.List<object[]>()
					.ForEach(row =>
					{
						row.ForEach(data =>
						{
							Console.WriteLine(data);
							Console.WriteLine();
						});
					});
			});

#endif
		}
	}
}