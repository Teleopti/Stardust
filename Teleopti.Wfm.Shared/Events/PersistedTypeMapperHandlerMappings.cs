using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	internal static class PersistedTypeMapperHandlerMappings
	{
		public static IEnumerable<PersistedTypeMapping> Mappings()
		{
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MessageBrokerMailboxPurger",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.MessageBroker.Server.MessageBrokerMailboxPurger, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.MessageBroker.Server.MessageBrokerMailboxPurger, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SendUpdateStaffingReadModelHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Staffing.SendUpdateStaffingReadModelHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Staffing.SendUpdateStaffingReadModelHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PublishInitializeReadModelEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.PublishInitializeReadModelEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.PublishInitializeReadModelEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RequeueHangfireEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.RequeueHangfireEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.RequeueHangfireEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "CleanFailedQueueHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.CleanFailedQueueHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.CleanFailedQueueHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonEmploymentChangedEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonEmploymentChangedEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonEmploymentChangedEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TerminatePersonHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.TerminatePersonHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.TerminatePersonHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ReadModelInitializeHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelInitializeHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ReadModelInitializeHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonAssociationChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonAssociationChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonAssociationChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "UpdateFindPersonDataHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.UpdateFindPersonDataHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.UpdateFindPersonDataHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "BuildInGroupsAnalyticsUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.BuildInGroupsAnalyticsUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.BuildInGroupsAnalyticsUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "GroupingReadModelDataUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.GroupingReadModelDataUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.GroupingReadModelDataUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsAbsenceUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Absence.AnalyticsAbsenceUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Absence.AnalyticsAbsenceUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsScenarioUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Scenario.AnalyticsScenarioUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Scenario.AnalyticsScenarioUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PreferenceFulfillmentChangedHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Preference.PreferenceFulfillmentChangedHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Preference.PreferenceFulfillmentChangedHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPreferenceUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Preference.AnalyticsPreferenceUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Preference.AnalyticsPreferenceUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsGroupPageUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.AnalyticsGroupPageUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.AnalyticsGroupPageUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "GroupingReadModelGroupPageUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.GroupingReadModelGroupPageUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.GroupingReadModelGroupPageUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsOvertimeUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers.AnalyticsOvertimeUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers.AnalyticsOvertimeUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "QuickForecastWorkloadsEventHandlerHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.QuickForecastWorkloadsEventHandlerHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.QuickForecastWorkloadsEventHandlerHangfire, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RecalculateForecastOnSkillEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.RecalculateForecastOnSkillEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.RecalculateForecastOnSkillEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsDayOffUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.DayOff.AnalyticsDayOffUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.DayOff.AnalyticsDayOffUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsActivityUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Activity.AnalyticsActivityUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Activity.AnalyticsActivityUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsRequestUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsWorkloadUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Workload.AnalyticsWorkloadUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Workload.AnalyticsWorkloadUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsForecastWorkloadUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastWorkloadUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastWorkloadUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsForecastUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonNameUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData.AnalyticsPersonNameUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData.AnalyticsPersonNameUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsBusinessUnitUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit.AnalyticsBusinessUnitUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit.AnalyticsBusinessUnitUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsSiteUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsSiteUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsSiteUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsTeamUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsTeamUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsTeamUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonLocalDateUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsPersonLocalDateUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsPersonLocalDateUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsDateChangedHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsDateChangedHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsDateChangedHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsTimeZoneUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsTimeZoneUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsTimeZoneUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsHourlyAvailabilityMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsHourlyAvailabilityMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsHourlyAvailabilityMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsOptionalColumnGroupPageHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsOptionalColumnGroupPageHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsOptionalColumnGroupPageHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPreferenceMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPreferenceMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPreferenceMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsRequestMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsRequestMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsRequestMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsScheduleMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonPeriodSkillsUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodSkillsUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodSkillsUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonGroupsHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonFinderReadOnlyUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.PersonFinderReadOnlyUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.PersonFinderReadOnlyUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsPersonPeriodUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "UpdateGroupingReadModelHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.UpdateGroupingReadModelHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.UpdateGroupingReadModelHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsSkillUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Skill.AnalyticsSkillUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Skill.AnalyticsSkillUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ShiftCategorySelectionModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.ShiftCategorySelectionModelUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.ShiftCategorySelectionModelUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsShiftCategoryUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.AnalyticsShiftCategoryUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.AnalyticsShiftCategoryUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ShiftTradeRequestHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade.ShiftTradeRequestHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade.ShiftTradeRequestHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleChangedInDefaultScenarioNotificationNew",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedInDefaultScenarioNotificationNew, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedInDefaultScenarioNotificationNew, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleChangedNotifierHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedNotifierHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedNotifierHangfire, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ProjectionChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleProjectionReadOnlyUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection.ScheduleProjectionReadOnlyUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection.ScheduleProjectionReadOnlyUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleReadModelWrapperHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleReadModelWrapperHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleReadModelWrapperHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ShiftExchangeOfferHandlerNew",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ShiftExchangeOfferHandlerNew, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ShiftExchangeOfferHandlerNew, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleDayReadModelHandlerHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleDayReadModelHandlerHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleDayReadModelHandlerHangfire, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PersonScheduleDayReadModelUpdaterHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel.PersonScheduleDayReadModelUpdaterHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel.PersonScheduleDayReadModelUpdaterHangfire, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsScheduleChangeUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AnalyticsAvailabilityUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Availability.AnalyticsAvailabilityUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Availability.AnalyticsAvailabilityUpdater, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PurgeAuditHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Audit.PurgeAuditHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Audit.PurgeAuditHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "WaitlistProcessPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.WaitlistProcessPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.WaitlistProcessPublisher, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AbsenceRequestQueueStrategyHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.AbsenceRequestQueueStrategyHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.AbsenceRequestQueueStrategyHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RequestPersonAbsenceRemovedEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.RequestPersonAbsenceRemovedEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.RequestPersonAbsenceRemovedEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "NewAbsenceReport",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.NewAbsenceReport, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.NewAbsenceReport, Teleopti.Ccc.Domain"
			};


			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AgentStateMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Domain.Service.AgentStateMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.AgentStateMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ScheduleChangeProcessor",
				LegacyPersistedNames = new[]
				{
					"Teleopti.Wfm.Adherence.Domain.Service.ScheduleChangeProcessor, Teleopti.Wfm.Adherence",
					"Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service.ScheduleChangeProcessor, Teleopti.Ccc.Domain"
				},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.ScheduleChangeProcessor, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "AgentStateReadModelMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.AgentStateReadModelMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Monitor.AgentStateReadModelMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "ExternalLogonReadModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.ExternalLogonReadModelUpdater, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.ExternalLogonReadModelUpdater, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HistoricalOverviewReadModelMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.HistoricalOverviewReadModelMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Historical.HistoricalOverviewReadModelMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "MappingReadModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.MappingReadModelUpdater, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.States.MappingReadModelUpdater, Teleopti.Wfm.Adherence"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "RtaEventStoreMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.RtaEventStoreMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Historical.RtaEventStoreMaintainer, Teleopti.Wfm.Adherence"
			};


			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HandlerDisabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerDisabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerDisabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HandlerMethodDisabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerMethodDisabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerMethodDisabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "PackageHandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PackageHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PackageHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "SameHandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SameHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SameHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "TriggerSkillForecastReadModelHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Forecasting.TriggerSkillForecastReadModel, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Forecasting.TriggerSkillForecastReadModelHandler, Teleopti.Ccc.Domain"
			};
		}
	}
}