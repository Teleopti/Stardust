using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class TypeMappingEventsTest : ITestInterceptor
	{
		public PersistedTypeMapperForTest Mapper;
		public IJsonEventSerializer Serializer;
		public IJsonEventDeserializer Deserializer;

		public void OnBefore()
		{
			Mapper.DynamicMappingsForTestProjects = false;
			Mapper.StaticMappingsForTestProjects = false;
		}

		[Test]
		public void AllNewEventsShouldBeMapped()
		{
			var events =
				from assembly in EventHandlerLocations.Assemblies()
				from type in assembly.GetTypes()
				where typeof(IEvent).IsAssignableFrom(type)
				select type;

			var exceptions = events
				.Select(x =>
				{
					try
					{
						Mapper.NameForPersistence(x)
							.Should().Not.Be.Empty();
						return null;
					}
					catch (ArgumentException e)
					{
						return e;
					}
				})
				.Where(x => x != null)
				.ToArray();

			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		[Test]
		public void AllMovedOrRemovedOrRenamedEventsShouldBeConsidered()
		{
			Mapper.AllCurrentTypeNames()
				.Select(x => Type.GetType(x, true))
				.ForEach(x => x.Should().Not.Be.Null());
		}

		[Test]
		public void AllKnownPersistedNames20190207houldWork()
		{
			var legacyTypes = new[]
			{
				"Teleopti.Ccc.Domain.ApplicationLayer.Event, Teleopti.Wfm.Shared",
				"Teleopti.Ccc.Domain.ApplicationLayer.Event, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.EventWithLogOnContext, Teleopti.Wfm.Shared",
				"Teleopti.Ccc.Domain.ApplicationLayer.EventWithLogOnContext, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent, Teleopti.Wfm.Shared",
				"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent[], Teleopti.Wfm.Shared",
				"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent[], Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Optimization.DayOffOptimizationWasOrdered, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.Events.SharedMinuteTickEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.Events.IntradayToolEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.Events.PublishInitializeReadModelEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.Events.RequeueHangfireEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.Events.IndexMaintenanceEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.Events.SharedHourTickEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Scheduling.WebScheduleStardustBaseEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Scheduling.WebClearScheduleStardustEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Scheduling.WebScheduleStardustEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Scheduling.IntradayOptimizationOnStardustWasOrdered, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.TestToggleEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.TestToggle2Event, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.EventWithInfrastructureContext, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Payroll.RunPayrollExportEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.ImportForecastProcessorMessage, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.OpenAndSplitTargetSkillMessage, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator.FixReadModelsEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator.ValidateReadModelsEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.MainShiftReplaceNotificationEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventBase, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventBase, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner.SchedulingWasOrdered, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner.IntradayOptimizationWasOrdered, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ForecastChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonEmploymentChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForShiftExchangeOffer, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.RefreshPayrollFormatsEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.UpdateSkillForecastReadModelEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.OptionalColumnValueChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffDeletedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateBadgeEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PossibleTimeZoneChangeEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportExternalPerformanceInfoEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProcessWaitlistedRequestsEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ReloadSchedules, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.StardustJobInfo, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportAgentEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsTimeZoneChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.CommonNameDescriptionChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportScheduleEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.CopyScheduleEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ManageScheduleBaseEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.OptionalColumnCollectionChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleBackoutEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantDayTickEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.UnknownStateCodeReceviedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.UpdateStaffingLevelReadModelEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.InitialLoadScheduleProjectionEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsDatesChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ApproveRequestsWithValidatorsEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.AvailabilityChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.BusinessUnitChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.MainShiftCategoryReplaceEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.NewMultiAbsenceRequestsCreatedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestCreatedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestDeletedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestChangedBase, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillDayChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.WorkloadChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.RequestPersonAbsenceRemovedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonalActivityAddedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetChangedBase, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetCreated, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetChanged, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetDeleted, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.AbsenceChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.AbsenceDeletedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScenarioChangeEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScenarioDeleteEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffTemplateChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ExportMultisiteSkillsToSkillEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ShiftCategoryChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ShiftCategoryDeletedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.AcceptShiftTradeEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsPersonPeriodSkillsChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.NewShiftTradeRequestCreatedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.NewAbsenceReportCreatedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceEventBase, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceCreatedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceDeletedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAssignmentLayerRemovedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.QuickForecastWorkloadsEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateForecastOnSkillCollectionEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ServiceBusHealthCheckEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillCreatedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillDeletedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantMinuteTickEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.GroupPageCollectionChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.SettingsForPersonPeriodChangedEvent, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Events.ActivityChangedEvent, Teleopti.Ccc.Domain",
			};

			legacyTypes.ForEach(x =>
			{
				Mapper.TypeForPersistedName(x)
					.Should().Not.Be.Null();
			});
		}

		[Test]
		public void AllEventsShouldSerializeAndDeserialize()
		{
			var eventTypes =
				from assembly in EventHandlerLocations.Assemblies()
				from type in assembly.GetTypes()
				where typeof(IEvent).IsAssignableFrom(type)
				where type.IsClass && !type.IsAbstract
				select type;
			var events = eventTypes
				.Select(materializeType)
				.ToArray();

			var exceptions = events
				.Select(x =>
				{
					try
					{
						Console.WriteLine(x.GetType().Name);
						var serialized = Serializer.SerializeEvent(x);
						Deserializer.DeserializeEvent(serialized, x.GetType());
						return null;
					}
					catch (ArgumentException e)
					{
						return e;
					}
				})
				.Where(x => x != null)
				.ToArray();

			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		private static object materializeType(Type type)
		{
			var instance = Activator.CreateInstance(type);
			
			var properties = from p in instance.GetType().GetProperties()
				let propertyType = p.PropertyType
				where typeof(IEnumerable).IsAssignableFrom(propertyType)
				where propertyType.IsGenericType
				where !propertyType.GenericTypeArguments[0].IsPrimitive
				where propertyType.GenericTypeArguments[0] != typeof(string)
				select p;
			properties.ForEach(x =>
			{
				var value = Array.CreateInstance(x.PropertyType.GenericTypeArguments[0], 1);
				var elementInstance = materializeType(x.PropertyType.GenericTypeArguments[0]);
				value.SetValue(elementInstance, 0);
				x.SetValue(instance, value);
			});
			
			return instance;
		}
	}
}