using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	internal static class PersistedTypeMapperEventMappings
	{
		public static IEnumerable<PersistedTypeMapping> Mappings()
		{
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "IEvent",
				LegacyPersistedNames = new[]
				{
					"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent, Teleopti.Wfm.Shared",
					"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent, Teleopti.Ccc.Domain"
				},
				CurrentTypeName = "Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent, Teleopti.Wfm.Shared"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "IEvent[]",
				LegacyPersistedNames = new[]
				{
					"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent[], Teleopti.Wfm.Shared",
					"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent[], Teleopti.Ccc.Domain"
				},
				CurrentTypeName = "Teleopti.Ccc.Domain.InterfaceLegacy.Domain.IEvent[], Teleopti.Wfm.Shared"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "Event",
				LegacyPersistedNames = new[]
				{
					"Teleopti.Ccc.Domain.ApplicationLayer.Event, Teleopti.Wfm.Shared",
					"Teleopti.Ccc.Domain.ApplicationLayer.Event, Teleopti.Ccc.Domain"
				},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Event, Teleopti.Wfm.Shared"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "EventWithLogOnContext",
				LegacyPersistedNames = new[]
				{
					"Teleopti.Ccc.Domain.ApplicationLayer.EventWithLogOnContext, Teleopti.Wfm.Shared",
					"Teleopti.Ccc.Domain.ApplicationLayer.EventWithLogOnContext, Teleopti.Ccc.Domain",
				},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.EventWithLogOnContext, Teleopti.Wfm.Shared"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "DayOffOptimizationWasOrdered",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Optimization.DayOffOptimizationWasOrdered, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Optimization.DayOffOptimizationWasOrdered, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SharedMinuteTickEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.Events.SharedMinuteTickEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.Events.SharedMinuteTickEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "IntradayToolEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.Events.IntradayToolEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.Events.IntradayToolEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PublishInitializeReadModelEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.Events.PublishInitializeReadModelEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.Events.PublishInitializeReadModelEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RequeueHangfireEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.Events.RequeueHangfireEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.Events.RequeueHangfireEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "IndexMaintenanceEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.Events.IndexMaintenanceEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.Events.IndexMaintenanceEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SharedHourTickEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.Events.SharedHourTickEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.Events.SharedHourTickEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "WebScheduleStardustBaseEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Scheduling.WebScheduleStardustBaseEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Scheduling.WebScheduleStardustBaseEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "WebClearScheduleStardustEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Scheduling.WebClearScheduleStardustEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Scheduling.WebClearScheduleStardustEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "WebScheduleStardustEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Scheduling.WebScheduleStardustEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Scheduling.WebScheduleStardustEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "IntradayOptimizationOnStardustWasOrdered",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Scheduling.IntradayOptimizationOnStardustWasOrdered, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Scheduling.IntradayOptimizationOnStardustWasOrdered, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TestToggleEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.TestToggleEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.TestToggleEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TestToggle2Event",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.TestToggle2Event, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.TestToggle2Event, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "EventWithInfrastructureContext",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.EventWithInfrastructureContext, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.EventWithInfrastructureContext, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RunPayrollExportEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Payroll.RunPayrollExportEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Payroll.RunPayrollExportEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ImportForecastProcessorMessage",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.ImportForecastProcessorMessage, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.ImportForecastProcessorMessage, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "OpenAndSplitTargetSkillMessage",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.OpenAndSplitTargetSkillMessage, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.OpenAndSplitTargetSkillMessage, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "FixReadModelsEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator.FixReadModelsEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator.FixReadModelsEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ValidateReadModelsEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator.ValidateReadModelsEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator.ValidateReadModelsEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MainShiftReplaceNotificationEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.MainShiftReplaceNotificationEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.MainShiftReplaceNotificationEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventBase",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventBase, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventBase, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleChangedEventBase",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventBase, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventBase, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SchedulingWasOrdered",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner.SchedulingWasOrdered, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner.SchedulingWasOrdered, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "IntradayOptimizationWasOrdered",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner.IntradayOptimizationWasOrdered, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner.IntradayOptimizationWasOrdered, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ForecastChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ForecastChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ForecastChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonEmploymentChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonEmploymentChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonEmploymentChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventForShiftExchangeOffer",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForShiftExchangeOffer, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForShiftExchangeOffer, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RefreshPayrollFormatsEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.RefreshPayrollFormatsEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.RefreshPayrollFormatsEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "UpdateSkillForecastReadModelEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.UpdateSkillForecastReadModelEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.UpdateSkillForecastReadModelEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "OptionalColumnValueChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.OptionalColumnValueChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.OptionalColumnValueChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "DayOffDeletedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffDeletedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffDeletedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RecalculateBadgeEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateBadgeEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateBadgeEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PossibleTimeZoneChangeEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PossibleTimeZoneChangeEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PossibleTimeZoneChangeEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ImportExternalPerformanceInfoEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportExternalPerformanceInfoEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportExternalPerformanceInfoEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProcessWaitlistedRequestsEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProcessWaitlistedRequestsEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ProcessWaitlistedRequestsEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ReloadSchedules",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ReloadSchedules, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ReloadSchedules, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "StardustJobInfo",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.StardustJobInfo, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.StardustJobInfo, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ImportAgentEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportAgentEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportAgentEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsTimeZoneChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsTimeZoneChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsTimeZoneChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "CommonNameDescriptionChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.CommonNameDescriptionChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.CommonNameDescriptionChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ImportScheduleEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportScheduleEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportScheduleEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "CopyScheduleEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.CopyScheduleEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.CopyScheduleEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ManageScheduleBaseEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ManageScheduleBaseEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ManageScheduleBaseEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "OptionalColumnCollectionChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.OptionalColumnCollectionChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.OptionalColumnCollectionChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleBackoutEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleBackoutEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleBackoutEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TenantDayTickEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantDayTickEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantDayTickEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "UnknownStateCodeReceviedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.UnknownStateCodeReceviedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.UnknownStateCodeReceviedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "UpdateStaffingLevelReadModelEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.UpdateStaffingLevelReadModelEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.UpdateStaffingLevelReadModelEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "InitialLoadScheduleProjectionEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.InitialLoadScheduleProjectionEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.InitialLoadScheduleProjectionEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsDatesChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsDatesChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsDatesChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ApproveRequestsWithValidatorsEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ApproveRequestsWithValidatorsEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ApproveRequestsWithValidatorsEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AvailabilityChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AvailabilityChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AvailabilityChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "BusinessUnitChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.BusinessUnitChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.BusinessUnitChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MainShiftCategoryReplaceEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.MainShiftCategoryReplaceEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.MainShiftCategoryReplaceEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "NewMultiAbsenceRequestsCreatedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.NewMultiAbsenceRequestsCreatedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.NewMultiAbsenceRequestsCreatedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonRequestCreatedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestCreatedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestCreatedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonRequestDeletedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestDeletedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestDeletedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonRequestChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonRequestChangedBase",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestChangedBase, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonRequestChangedBase, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SkillDayChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillDayChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillDayChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "WorkloadChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.WorkloadChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.WorkloadChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RequestPersonAbsenceRemovedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.RequestPersonAbsenceRemovedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.RequestPersonAbsenceRemovedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonalActivityAddedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonalActivityAddedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonalActivityAddedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MultiplicatorDefinitionSetChangedBase",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetChangedBase, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetChangedBase, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MultiplicatorDefinitionSetCreated",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetCreated, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetCreated, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MultiplicatorDefinitionSetChanged",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetChanged, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetChanged, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MultiplicatorDefinitionSetDeleted",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetDeleted, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.MultiplicatorDefinitionSetDeleted, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AbsenceChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AbsenceChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AbsenceChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AbsenceDeletedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AbsenceDeletedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AbsenceDeletedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScenarioChangeEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScenarioChangeEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScenarioChangeEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScenarioDeleteEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScenarioDeleteEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScenarioDeleteEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "DayOffTemplateChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffTemplateChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffTemplateChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ExportMultisiteSkillsToSkillEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ExportMultisiteSkillsToSkillEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ExportMultisiteSkillsToSkillEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ShiftCategoryChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ShiftCategoryChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ShiftCategoryChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ShiftCategoryDeletedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ShiftCategoryDeletedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ShiftCategoryDeletedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AcceptShiftTradeEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AcceptShiftTradeEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AcceptShiftTradeEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonPeriodSkillsChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsPersonPeriodSkillsChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsPersonPeriodSkillsChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "NewShiftTradeRequestCreatedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.NewShiftTradeRequestCreatedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.NewShiftTradeRequestCreatedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "NewAbsenceReportCreatedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.NewAbsenceReportCreatedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.NewAbsenceReportCreatedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PreferenceEventBase",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceEventBase, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceEventBase, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PreferenceChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PreferenceCreatedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceCreatedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceCreatedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PreferenceDeletedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceDeletedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PreferenceDeletedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonAssignmentLayerRemovedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAssignmentLayerRemovedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAssignmentLayerRemovedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "QuickForecastWorkloadsEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.QuickForecastWorkloadsEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.QuickForecastWorkloadsEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RecalculateForecastOnSkillCollectionEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateForecastOnSkillCollectionEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateForecastOnSkillCollectionEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ServiceBusHealthCheckEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ServiceBusHealthCheckEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ServiceBusHealthCheckEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SkillCreatedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillCreatedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillCreatedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SkillDeletedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillDeletedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillDeletedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SkillChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.SkillChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TenantMinuteTickEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantMinuteTickEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantMinuteTickEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "GroupPageCollectionChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.GroupPageCollectionChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.GroupPageCollectionChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SettingsForPersonPeriodChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.SettingsForPersonPeriodChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.SettingsForPersonPeriodChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ActivityChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ActivityChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ActivityChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ActivityMovedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ActivityMovedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ActivityMovedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "DayOffAddedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffAddedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.DayOffAddedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "DayUnscheduledEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.DayUnscheduledEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.DayUnscheduledEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleOnNode",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleOnNode, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleOnNode, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "StardustHealthCheckEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.StardustHealthCheckEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.StardustHealthCheckEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TenantHourTickEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantHourTickEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantHourTickEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonAbsenceModifiedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAbsenceModifiedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAbsenceModifiedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonAbsenceAddedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAbsenceAddedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAbsenceAddedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonCollectionChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonCollectionChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonCollectionChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonCollectionChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsPersonCollectionChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsPersonCollectionChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonPeriodRangeChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsPersonPeriodRangeChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.AnalyticsPersonPeriodRangeChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonCollectionChangedEventBase",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonCollectionChangedEventBase, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonCollectionChangedEventBase, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonDeletedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonDeletedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonDeletedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonTeamChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonTeamChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonTeamChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonEmploymentNumberChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonEmploymentNumberChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonEmploymentNumberChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonNameChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonNameChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonNameChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TeamNameChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.TeamNameChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.TeamNameChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SiteNameChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.SiteNameChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.SiteNameChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonTerminalDateChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonTerminalDateChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonTerminalDateChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduledResourcesChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduledResourcesChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduledResourcesChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ImportForecastsFileToSkillEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportForecastsFileToSkillEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ImportForecastsFileToSkillEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonAbsenceRemovedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAbsenceRemovedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAbsenceRemovedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonAssociationChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAssociationChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonAssociationChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonAssociationChangedEvent.ExternalLogon",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon[], Teleopti.Ccc.Domain"
			};


			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventNew",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventNew, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventNew, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventForPersonScheduleDay",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForPersonScheduleDay, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForPersonScheduleDay, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventForScheduleDay",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForScheduleDay, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForScheduleDay, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventForScheduleProjection",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForScheduleProjection, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForScheduleProjection, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleInitializeTriggeredEventForPersonScheduleDay",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleInitializeTriggeredEventForPersonScheduleDay, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleInitializeTriggeredEventForPersonScheduleDay, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleInitializeTriggeredEventForScheduleDay",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleInitializeTriggeredEventForScheduleDay, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleInitializeTriggeredEventForScheduleDay, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleInitializeTriggeredEventForScheduleProjection",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleInitializeTriggeredEventForScheduleProjection, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ScheduleInitializeTriggeredEventForScheduleProjection, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonPeriodChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonPeriodChangedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.PersonPeriodChangedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ActivityAddedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ActivityAddedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ActivityAddedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "FullDayAbsenceAddedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.FullDayAbsenceAddedEvent, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.FullDayAbsenceAddedEvent, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ActivityStart",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonActivityStartEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonActivityStartEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AdherenceDayStart",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonAdherenceDayStartEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonAdherenceDayStartEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ArrivedLateForWork",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonArrivedLateForWorkEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonArrivedLateForWorkEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "InAdherence",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonInAdherenceEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonInAdherenceEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "NeutralAdherence",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonNeutralAdherenceEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonNeutralAdherenceEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "OutOfAdherence",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonOutOfAdherenceEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonOutOfAdherenceEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RuleChanged",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonRuleChangedEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonRuleChangedEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ShiftEnd",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonShiftEndEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonShiftEndEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ShiftStart",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonShiftStartEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonShiftStartEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "StateChanged",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.Events.PersonStateChangedEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.Events.PersonStateChangedEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AgentStateChanged",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Monitor.AgentStateChangedEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Monitor.AgentStateChangedEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AdjustedToNeutralAdherence",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Historical.Events.PeriodAdjustedToNeutralEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Historical.Events.PeriodAdjustedToNeutralEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ApprovedPeriodRemoved",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Historical.Events.ApprovedPeriodRemovedEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Historical.Events.ApprovedPeriodRemovedEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ApprovedAsInAdherence",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Historical.Events.PeriodApprovedAsInAdherenceEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Historical.Events.PeriodApprovedAsInAdherenceEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RtaMapChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Configuration.Events.RtaMapChangedEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Configuration.Events.RtaMapChangedEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RtaRuleChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Configuration.Events.RtaRuleChangedEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Configuration.Events.RtaRuleChangedEvent, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RtaStateGroupChangedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Configuration.Events.RtaStateGroupChangedEvent, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Configuration.Events.RtaStateGroupChangedEvent, Teleopti.Wfm.Adherence"
			};

			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "LockInfo[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Scheduling.Legacy.Commands.LockInfo[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Scheduling.Legacy.Commands.LockInfo[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ForecastsRow[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Forecasting.ForecastsFile.ForecastsRow[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Forecasting.ForecastsFile.ForecastsRow[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventForShiftExchangeOfferDateAndChecksums[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForShiftExchangeOfferDateAndChecksums[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.ProjectionChangedEventForShiftExchangeOfferDateAndChecksums[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "DateOnly[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MultisiteSkillSelection[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General.MultisiteSkillSelection[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General.MultisiteSkillSelection[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RecalculateForecastOnSkill[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateForecastOnSkill[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Events.RecalculateForecastOnSkill[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventScheduleDay[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventScheduleDay[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventScheduleDay[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ChildSkillSelection[]",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General.ChildSkillSelection[], Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General.ChildSkillSelection[], Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduledActivity[]",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.States.ScheduledActivity[], Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.ScheduledActivity[], Teleopti.Wfm.Adherence"
			};
		}
	}
}