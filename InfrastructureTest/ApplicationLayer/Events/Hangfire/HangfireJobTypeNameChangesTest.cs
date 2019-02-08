using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
			Publisher.Publish(new MovedEvent {Data = "data"});
			simulateLegacyTypeFor<MovedEvent>("Teleopti.Original.Assembly.Namespace.MovedEventName, Teleopti.Original");

			Hangfire.Value.EmulateWorkerIteration();

			Handler.Got.Should().Be("data");
		}

		public class MovedEvent : IEvent
		{
			public string Data { get; set; }
		}

		public class TestHandler :
			IHandleEvent<MovedEvent>,
			IRunOnHangfire
		{
			public string Got;

			public void Handle(MovedEvent @event)
			{
				Got = @event.Data;
			}
		}

		private void simulateLegacyTypeFor<T>(string legacyName)
		{
			var persistedType = typeof(T);
			var persistedName = Mapper.NameForPersistence(persistedType);
			outputJobInfo();
			Analytics.Do(x =>
			{
				x.Current().FetchSession()
					.CreateSQLQuery($@"
UPDATE Hangfire.Job 
SET 
	InvocationData = REPLACE(InvocationData, '{persistedName}', '{legacyName}'),
	Arguments = REPLACE(Arguments, '{persistedName}', '{legacyName}')
")
					.ExecuteUpdate();
			});
			outputJobInfo();
		}

		private void outputJobInfo()
		{
//			Analytics.Do(x =>
//			{
//				x.Current().FetchSession()
//					.CreateSQLQuery("SELECT InvocationData, Arguments FROM Hangfire.Job")
//					.List<object[]>()
//					.ForEach(row =>
//					{
//						row.ForEach(data =>
//						{
//							Console.WriteLine();
//							Console.WriteLine(data);
//						});
//					});
//			});
		}
	}
}