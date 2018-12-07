using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HandlerTypeMapper
	{
		public string NameForPersistence(Type handlerType)
		{
			var typeName = $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}";
			if (!persistedNameForTypeName.TryGetValue(typeName, out var persistedName))
				throw new ArgumentException($"{typeName} is not mapped");
			return persistedName;

			//return $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}";
		}

		public Type TypeForPersistedName(string persistedName)
		{
			if (!typeNameForPersistedName.TryGetValue(persistedName, out var typeName))
				throw new ArgumentException($"{persistedName} is not mapped");
			return Type.GetType(typeName, true);

			//return Type.GetType(handlerTypeId, true);
		}


		private class mappingSpec
		{
			public string CurrentPersistedName;
			public IEnumerable<string> LegacyPersistedNames = Enumerable.Empty<string>();

			public string CurrentTypeName;
		}

		private static readonly IEnumerable<mappingSpec> mappings = makeMappings();

		private static readonly IDictionary<string, string> persistedNameForTypeName =
			mappings.ToDictionary(x => x.CurrentTypeName, x => x.CurrentPersistedName);

		private static readonly IDictionary<string, string> typeNameForPersistedName = makeTypeNameForPersistedName();

		private static IDictionary<string, string> makeTypeNameForPersistedName()
		{
			return (
					from m in mappings
					let allPersistedNames = m.CurrentPersistedName.AsArray().Concat(m.LegacyPersistedNames)
					from persistedName in allPersistedNames
					select new
					{
						persistedName,
						m.CurrentTypeName
					}
				)
				.ToDictionary(x => x.persistedName, x => x.CurrentTypeName);
		}

		private static IEnumerable<mappingSpec> makeMappings()
		{
			yield return new mappingSpec
			{
				CurrentPersistedName = "MessageBrokerMailboxPurger",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.MessageBroker.Server.MessageBrokerMailboxPurger, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.MessageBroker.Server.MessageBrokerMailboxPurger, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "SendUpdateStaffingReadModelHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Staffing.SendUpdateStaffingReadModelHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Staffing.SendUpdateStaffingReadModelHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PublishInitializeReadModelEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.PublishInitializeReadModelEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.PublishInitializeReadModelEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "RequeueHangfireEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.RequeueHangfireEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.RequeueHangfireEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "CleanFailedQueueHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.CleanFailedQueueHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.CleanFailedQueueHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PersonEmploymentChangedEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonEmploymentChangedEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonEmploymentChangedEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TerminatePersonHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.TerminatePersonHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.TerminatePersonHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ReadModelInitializeHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelInitializeHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ReadModelInitializeHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PersonAssociationChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonAssociationChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonAssociationChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "HandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "HandlerDisabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerDisabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerDisabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "HandlerMethodDisabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerMethodDisabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerMethodDisabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PackageHandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PackageHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PackageHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "SameHandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SameHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SameHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "UpdateFindPersonDataHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.UpdateFindPersonDataHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.UpdateFindPersonDataHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "BuildInGroupsAnalyticsUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.BuildInGroupsAnalyticsUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.BuildInGroupsAnalyticsUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "GroupingReadModelDataUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.GroupingReadModelDataUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.GroupingReadModelDataUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsAbsenceUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Absence.AnalyticsAbsenceUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Absence.AnalyticsAbsenceUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsScenarioUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Scenario.AnalyticsScenarioUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Scenario.AnalyticsScenarioUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PreferenceFulfillmentChangedHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Preference.PreferenceFulfillmentChangedHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Preference.PreferenceFulfillmentChangedHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsPreferenceUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Preference.AnalyticsPreferenceUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Preference.AnalyticsPreferenceUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsGroupPageUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.AnalyticsGroupPageUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.AnalyticsGroupPageUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "GroupingReadModelGroupPageUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.GroupingReadModelGroupPageUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.GroupingReadModelGroupPageUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsOvertimeUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers.AnalyticsOvertimeUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers.AnalyticsOvertimeUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "QuickForecastWorkloadsEventHandlerHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.QuickForecastWorkloadsEventHandlerHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.QuickForecastWorkloadsEventHandlerHangfire, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "RecalculateForecastOnSkillEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.RecalculateForecastOnSkillEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.RecalculateForecastOnSkillEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsDayOffUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.DayOff.AnalyticsDayOffUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.DayOff.AnalyticsDayOffUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsActivityUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Activity.AnalyticsActivityUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Activity.AnalyticsActivityUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsRequestUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsWorkloadUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Workload.AnalyticsWorkloadUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Workload.AnalyticsWorkloadUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsForecastWorkloadUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastWorkloadUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastWorkloadUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonNameUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData.AnalyticsPersonNameUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData.AnalyticsPersonNameUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsBusinessUnitUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit.AnalyticsBusinessUnitUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit.AnalyticsBusinessUnitUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsSiteUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsSiteUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsSiteUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsTeamUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsTeamUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsTeamUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonLocalDateUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsPersonLocalDateUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsPersonLocalDateUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsDateChangedHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsDateChangedHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsDateChangedHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsTimeZoneUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsTimeZoneUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsTimeZoneUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsHourlyAvailabilityMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsHourlyAvailabilityMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsHourlyAvailabilityMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsOptionalColumnGroupPageHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsOptionalColumnGroupPageHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsOptionalColumnGroupPageHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsPreferenceMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPreferenceMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPreferenceMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsRequestMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsRequestMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsRequestMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsScheduleMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonPeriodSkillsUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodSkillsUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodSkillsUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonGroupsHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PersonFinderReadOnlyUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.PersonFinderReadOnlyUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.PersonFinderReadOnlyUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonPeriodUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "UpdateGroupingReadModelHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.UpdateGroupingReadModelHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.UpdateGroupingReadModelHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsSkillUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Skill.AnalyticsSkillUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Skill.AnalyticsSkillUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ShiftCategorySelectionModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.ShiftCategorySelectionModelUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.ShiftCategorySelectionModelUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsShiftCategoryUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.AnalyticsShiftCategoryUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.AnalyticsShiftCategoryUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ShiftTradeRequestHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade.ShiftTradeRequestHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade.ShiftTradeRequestHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ScheduleChangedInDefaultScenarioNotificationNew",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedInDefaultScenarioNotificationNew, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedInDefaultScenarioNotificationNew, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ScheduleChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ScheduleChangedNotifierHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedNotifierHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedNotifierHangfire, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ProjectionChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ScheduleProjectionReadOnlyUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection.ScheduleProjectionReadOnlyUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection.ScheduleProjectionReadOnlyUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ScheduleReadModelWrapperHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleReadModelWrapperHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleReadModelWrapperHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ShiftExchangeOfferHandlerNew",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ShiftExchangeOfferHandlerNew, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ShiftExchangeOfferHandlerNew, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ScheduleDayReadModelHandlerHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleDayReadModelHandlerHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleDayReadModelHandlerHangfire, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PersonScheduleDayReadModelUpdaterHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel.PersonScheduleDayReadModelUpdaterHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel.PersonScheduleDayReadModelUpdaterHangfire, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsScheduleChangeUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AnalyticsAvailabilityUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Availability.AnalyticsAvailabilityUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Availability.AnalyticsAvailabilityUpdater, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "PurgeAuditHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Audit.PurgeAuditHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Audit.PurgeAuditHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "WaitlistProcessPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.WaitlistProcessPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.WaitlistProcessPublisher, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AbsenceRequestQueueStrategyHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.AbsenceRequestQueueStrategyHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.AbsenceRequestQueueStrategyHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "RequestPersonAbsenceRemovedEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.RequestPersonAbsenceRemovedEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.RequestPersonAbsenceRemovedEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "NewAbsenceReport",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.NewAbsenceReport, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.NewAbsenceReport, Teleopti.Ccc.Domain"
			};


			yield return new mappingSpec
			{
				CurrentPersistedName = "AgentStateMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Domain.Service.AgentStateMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Domain.Service.AgentStateMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ScheduleChangeProcessor",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Domain.Service.ScheduleChangeProcessor, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Domain.Service.ScheduleChangeProcessor, Teleopti.Wfm.Adherence"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "AgentStateReadModelMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.AgentStateReadModelMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.AgentStateReadModelMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "ExternalLogonReadModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.ExternalLogonReadModelUpdater, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.ExternalLogonReadModelUpdater, Teleopti.Wfm.Adherence"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "HistoricalOverviewReadModelMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.HistoricalOverviewReadModelMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.HistoricalOverviewReadModelMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "MappingReadModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.MappingReadModelUpdater, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.MappingReadModelUpdater, Teleopti.Wfm.Adherence"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "RtaEventStoreMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.RtaEventStoreMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.RtaEventStoreMaintainer, Teleopti.Wfm.Adherence"
			};


			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler1",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventRealPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler2",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireAllowRecurringFailuresEventPublishingConcurrencyTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler3",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireAllowRecurringFailuresEventPublishingTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler4",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPackagePublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler5",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler6",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireAllowRecurringFailuresEventPublishingTest+FailingHandlerImpl2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler7",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestMultiHandler2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler8",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestBothHangfireHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler9",
				LegacyPersistedNames = new[]{"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler10",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AspectedHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AspectedHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler11",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AnotherHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AnotherHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler12",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestMultiHandler1, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler13",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventShortNameSerializationTest+FakeHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventShortNameSerializationTest+FakeHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler14",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireQueueOrderTest+QueueScheduleChangesTodayHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler15",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireQueueOrderTest+QueueCriticalScheduleChangesTodayHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler16",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireQueueOrderTest+QueueDefaultHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler17",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.QueuingHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler18a",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestMultiHandler1, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler18",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestMultiHandler2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler19",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler20a",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler20",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler21",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRetryEventPublishingConcurrencyTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler22",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRetryEventPublishingTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler23",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireSerializeEventPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "TestHandler24",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.MultiEventPublishingTest+HangfireEventHandler, Teleopti.Ccc.InfrastructureTest"
			};
			
			
			yield return new mappingSpec
			{
				CurrentPersistedName = "HangfireJobFailuresTest+TestHandler",
				CurrentTypeName = "Teleopti.Wfm.Administration.IntegrationTest.Hangfire.HangfireJobFailuresTest+TestHandler, Teleopti.Wfm.Administration.IntegrationTest"
			};
			yield return new mappingSpec
			{
				CurrentPersistedName = "HangfireJobPerformanceTest+TestHandler",
				CurrentTypeName = "Teleopti.Wfm.Administration.IntegrationTest.Hangfire.HangfireJobPerformanceTest+TestHandler, Teleopti.Wfm.Administration.IntegrationTest"
			};
		}
	}
}