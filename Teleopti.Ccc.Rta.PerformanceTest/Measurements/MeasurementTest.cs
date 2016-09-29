using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.Rta.TestApplication.TeleoptiRtaService;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest.Measurements
{
	[TestFixture]
	[RtaPerformanceTest]
	[Toggle(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
	[Toggle(Toggles.RTA_Optimize_39667)]
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	[Toggle(Toggles.RTA_BatchConnectionOptimization_40116)]
	[Toggle(Toggles.RTA_BatchQueryOptimization_40169)]
	[Toggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
	[Toggle(Toggles.RTA_ScheduleQueryOptimization_40260)]
	[Toggle(Toggles.RTA_ConnectionQueryOptimizeAllTheThings_40262)]
	[Toggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
	[Explicit]
	public class MeasurementTest : ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public ConfigurableSyncEventPublisher Publisher;
		public IDataSourceScope Tenant;
		public Http Http;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void BatchTest()
		{
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
			Publisher.AddHandler<AgentStateMaintainer>();
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithDefaultScenario("default")
				.WithStateGroup("phone")
				.WithStateCode("phone");
			Enumerable.Range(0, 100).ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));
			var userCodes = Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
			userCodes.ForEach(x => Database.WithAgent(x));
			Publisher.Publish(new TenantMinuteTickEvent());
			var states = 20000;

			var service = new TeleoptiRtaService {Url = TestSiteConfigurationSetup.URL + "/TeleoptiRtaService.svc"};

			var results = (
				from api in new[] { "wcf", "json"}
				from batchSize in new[] {500, 1000, 2500, 5000}
				from variation in new[] {"A", "B", "C"}
				select new {api, batchSize, variation}).Select(x =>
			{
				var batches = Enumerable.Range(0, states)
					.Batch(x.batchSize)
					.Select(_ => new BatchForTest
					{
						States = userCodes
							.Randomize()
							.Take(x.batchSize)
							.Select(y => new BatchStateForTest
							{
								UserCode = y,
								StateCode = "phone"
							})
					}).ToArray();

				var wcfBatches = batches
					.Select(b =>
					{
						return new
						{
							b.AuthenticationKey,
							b.PlatformTypeId,
							b.SourceId,
							States = b.States
								.Select(s => new ExternalUserState
								{
									IsLoggedOn = true,
									StateCode = s.StateCode,
									StateDescription = s.StateDescription,
									UserCode = s.UserCode
								}).ToArray()
						};
					}).ToArray();

				var jsonBatches = batches
					.Select(b =>
					{
						return b.States
							.Select(s => new ExternalUserStateWebModel
							{
								AuthenticationKey = b.AuthenticationKey,
								UserCode = s.UserCode,
								StateCode = s.StateCode,
								PlatformTypeId = b.PlatformTypeId,
								SourceId = b.SourceId,
							});
					});

				var timer = new Stopwatch();
				timer.Start();

				Exception exception = null;
				try
				{
					if (x.api == "json")
						jsonBatches.ForEach(b => Http.PostJson("Rta/State/Batch", b));
					else
						wcfBatches.ForEach(b =>
						{
							int result;
							bool resultSpecified;
							service.SaveBatchExternalUserState(b.AuthenticationKey, b.PlatformTypeId, b.SourceId, b.States, out result, out resultSpecified);
							if (result < 0)
								throw new Exception(result.ToString());
						});
				}
				catch (Exception e)
				{
					exception = e;
				}

				timer.Stop();

				return new
				{
					x.api,
					timer.Elapsed,
					stateTime = new TimeSpan(timer.Elapsed.Ticks/states),
					x.batchSize,
					x.variation,
					exception
				};
			});

			results
				.OrderBy(x => x.Elapsed)
				.ForEach(x => Console.WriteLine($"{x.api} {x.Elapsed} - {x.stateTime} {x.batchSize} {x.variation} {x.exception}"));
		}
	
	}
}