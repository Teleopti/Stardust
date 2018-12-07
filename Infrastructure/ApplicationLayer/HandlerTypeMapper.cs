using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HandlerTypeMapperForTest : HandlerTypeMapper
	{
		public IEnumerable<string> AllCurrentTypeNames() =>
			Mappings.Select(x => x.CurrentTypeName);

		protected override string ExceptionInfoFor(string typeName)
		{
			var persistedName = new Regex(@"\.([a-zA-Z]+)\,").Match(typeName).Groups[1];
			return $@"
yield return new mappingSpec
{{
	CurrentPersistedName = ""{persistedName}"",
	CurrentTypeName = ""{typeName}""
}};";
		}
	}

	public class HandlerTypeMapper
	{
		public string NameForPersistence(Type handlerType)
		{
			var typeName = $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}";
			if (!persistedNameForTypeName.TryGetValue(typeName, out var persistedName))
				throw new ArgumentException($"{typeName} is not mapped. {ExceptionInfoFor(typeName)}");
			return persistedName;
		}

		protected virtual string ExceptionInfoFor(string typeName) => null;

		public Type TypeForPersistedName(string persistedName)
		{
			if (!typeNameForPersistedName.TryGetValue(persistedName, out var typeName))
				throw new ArgumentException($"{persistedName} is not mapped");
			return Type.GetType(typeName, true);
		}

		protected static readonly IEnumerable<MappingSpec> Mappings = makeMappings();

		private static readonly IDictionary<string, string> persistedNameForTypeName =
			Mappings.ToDictionary(x => x.CurrentTypeName, x => x.CurrentPersistedName);

		private static readonly IDictionary<string, string> typeNameForPersistedName =
			(
				from m in Mappings
				let allPersistedNames = m.CurrentPersistedName.AsArray().Concat(m.LegacyPersistedNames)
				from persistedName in allPersistedNames
				select new
				{
					persistedName,
					m.CurrentTypeName
				}
			)
			.ToDictionary(x => x.persistedName, x => x.CurrentTypeName);


		private static IEnumerable<MappingSpec> makeMappings()
		{
			yield return new MappingSpec
			{
				CurrentPersistedName = "MessageBrokerMailboxPurger",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.MessageBroker.Server.MessageBrokerMailboxPurger, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.MessageBroker.Server.MessageBrokerMailboxPurger, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "SendUpdateStaffingReadModelHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Staffing.SendUpdateStaffingReadModelHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Staffing.SendUpdateStaffingReadModelHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PublishInitializeReadModelEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.PublishInitializeReadModelEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.PublishInitializeReadModelEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "RequeueHangfireEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.RequeueHangfireEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.RequeueHangfireEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "CleanFailedQueueHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.Infrastructure.CleanFailedQueueHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.Infrastructure.CleanFailedQueueHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PersonEmploymentChangedEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonEmploymentChangedEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonEmploymentChangedEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TerminatePersonHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.TerminatePersonHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.TerminatePersonHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ReadModelInitializeHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelInitializeHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ReadModelInitializeHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PersonAssociationChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonAssociationChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonAssociationChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "HandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "HandlerDisabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerDisabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerDisabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "HandlerMethodDisabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.HandlerMethodDisabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.HandlerMethodDisabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PackageHandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PackageHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PackageHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "SameHandlerEnabledByTestToggle",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SameHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SameHandlerEnabledByTestToggle, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "UpdateFindPersonDataHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.UpdateFindPersonDataHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.UpdateFindPersonDataHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "BuildInGroupsAnalyticsUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.BuildInGroupsAnalyticsUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.BuildInGroupsAnalyticsUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "GroupingReadModelDataUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.GroupingReadModelDataUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.GroupingReadModelDataUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsAbsenceUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Absence.AnalyticsAbsenceUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Absence.AnalyticsAbsenceUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsScenarioUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Scenario.AnalyticsScenarioUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Scenario.AnalyticsScenarioUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PreferenceFulfillmentChangedHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Preference.PreferenceFulfillmentChangedHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Preference.PreferenceFulfillmentChangedHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsPreferenceUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Preference.AnalyticsPreferenceUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Preference.AnalyticsPreferenceUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsGroupPageUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.AnalyticsGroupPageUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.AnalyticsGroupPageUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "GroupingReadModelGroupPageUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.GroupingReadModelGroupPageUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.GroupingReadModelGroupPageUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsOvertimeUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers.AnalyticsOvertimeUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers.AnalyticsOvertimeUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "QuickForecastWorkloadsEventHandlerHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.QuickForecastWorkloadsEventHandlerHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.QuickForecastWorkloadsEventHandlerHangfire, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "RecalculateForecastOnSkillEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.RecalculateForecastOnSkillEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Forecast.RecalculateForecastOnSkillEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsDayOffUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.DayOff.AnalyticsDayOffUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.DayOff.AnalyticsDayOffUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsActivityUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Activity.AnalyticsActivityUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Activity.AnalyticsActivityUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsRequestUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsWorkloadUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Workload.AnalyticsWorkloadUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Workload.AnalyticsWorkloadUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsForecastWorkloadUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastWorkloadUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastWorkloadUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonNameUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData.AnalyticsPersonNameUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData.AnalyticsPersonNameUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsBusinessUnitUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit.AnalyticsBusinessUnitUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit.AnalyticsBusinessUnitUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsSiteUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsSiteUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsSiteUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsTeamUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsTeamUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsTeamUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonLocalDateUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsPersonLocalDateUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsPersonLocalDateUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsDateChangedHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsDateChangedHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsDateChangedHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsTimeZoneUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsTimeZoneUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsTimeZoneUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsHourlyAvailabilityMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsHourlyAvailabilityMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsHourlyAvailabilityMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsOptionalColumnGroupPageHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsOptionalColumnGroupPageHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsOptionalColumnGroupPageHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsPreferenceMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPreferenceMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPreferenceMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsRequestMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsRequestMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsRequestMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsScheduleMatchingPerson",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonPeriodSkillsUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodSkillsUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodSkillsUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonGroupsHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PersonFinderReadOnlyUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.PersonFinderReadOnlyUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.PersonFinderReadOnlyUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsPersonPeriodUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "UpdateGroupingReadModelHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.UpdateGroupingReadModelHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.UpdateGroupingReadModelHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsSkillUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Skill.AnalyticsSkillUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Skill.AnalyticsSkillUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ShiftCategorySelectionModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.ShiftCategorySelectionModelUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.ShiftCategorySelectionModelUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsShiftCategoryUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.AnalyticsShiftCategoryUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.AnalyticsShiftCategoryUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ShiftTradeRequestHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade.ShiftTradeRequestHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade.ShiftTradeRequestHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ScheduleChangedInDefaultScenarioNotificationNew",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedInDefaultScenarioNotificationNew, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedInDefaultScenarioNotificationNew, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ScheduleChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ScheduleChangedNotifierHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedNotifierHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedNotifierHangfire, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ProjectionChangedEventPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventPublisher, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ScheduleProjectionReadOnlyUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection.ScheduleProjectionReadOnlyUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection.ScheduleProjectionReadOnlyUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ScheduleReadModelWrapperHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleReadModelWrapperHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleReadModelWrapperHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ShiftExchangeOfferHandlerNew",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ShiftExchangeOfferHandlerNew, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ShiftExchangeOfferHandlerNew, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ScheduleDayReadModelHandlerHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleDayReadModelHandlerHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleDayReadModelHandlerHangfire, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PersonScheduleDayReadModelUpdaterHangfire",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel.PersonScheduleDayReadModelUpdaterHangfire, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel.PersonScheduleDayReadModelUpdaterHangfire, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsScheduleChangeUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AnalyticsAvailabilityUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Availability.AnalyticsAvailabilityUpdater, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Availability.AnalyticsAvailabilityUpdater, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "PurgeAuditHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.Audit.PurgeAuditHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.Audit.PurgeAuditHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "WaitlistProcessPublisher",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.WaitlistProcessPublisher, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.WaitlistProcessPublisher, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AbsenceRequestQueueStrategyHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.AbsenceRequestQueueStrategyHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.AbsenceRequestQueueStrategyHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "RequestPersonAbsenceRemovedEventHandler",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.RequestPersonAbsenceRemovedEventHandler, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.RequestPersonAbsenceRemovedEventHandler, Teleopti.Ccc.Domain"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "NewAbsenceReport",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.NewAbsenceReport, Teleopti.Ccc.Domain"},
				CurrentTypeName = "Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.NewAbsenceReport, Teleopti.Ccc.Domain"
			};


			yield return new MappingSpec
			{
				CurrentPersistedName = "AgentStateMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Domain.Service.AgentStateMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Domain.Service.AgentStateMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ScheduleChangeProcessor",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.Domain.Service.ScheduleChangeProcessor, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.Domain.Service.ScheduleChangeProcessor, Teleopti.Wfm.Adherence"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "AgentStateReadModelMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.AgentStateReadModelMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.AgentStateReadModelMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "ExternalLogonReadModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.ExternalLogonReadModelUpdater, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.ExternalLogonReadModelUpdater, Teleopti.Wfm.Adherence"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "HistoricalOverviewReadModelMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.HistoricalOverviewReadModelMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.HistoricalOverviewReadModelMaintainer, Teleopti.Wfm.Adherence"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "MappingReadModelUpdater",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.MappingReadModelUpdater, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.MappingReadModelUpdater, Teleopti.Wfm.Adherence"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "RtaEventStoreMaintainer",
				LegacyPersistedNames = new[] {"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.RtaEventStoreMaintainer, Teleopti.Wfm.Adherence"},
				CurrentTypeName = "Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.RtaEventStoreMaintainer, Teleopti.Wfm.Adherence"
			};


			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler1",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventRealPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler2",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireAllowRecurringFailuresEventPublishingConcurrencyTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler3",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireAllowRecurringFailuresEventPublishingTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler4",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPackagePublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler5",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler6",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireAllowRecurringFailuresEventPublishingTest+FailingHandlerImpl2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler7",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestMultiHandler2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler8",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestBothHangfireHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler9",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler10",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AspectedHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AspectedHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler11",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AnotherHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventServerTest+AnotherHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler12",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestMultiHandler1, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler13",
				LegacyPersistedNames = new[] {"Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventShortNameSerializationTest+FakeHandler, Teleopti.Ccc.InfrastructureTest"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventShortNameSerializationTest+FakeHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler14",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireQueueOrderTest+QueueScheduleChangesTodayHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler15",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireQueueOrderTest+QueueCriticalScheduleChangesTodayHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler16",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireQueueOrderTest+QueueDefaultHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler17",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.QueuingHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler18a",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestMultiHandler1, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler18",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestMultiHandler2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler19",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler20a",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler20",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRecurringEventPublisherTest+TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler21",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRetryEventPublishingConcurrencyTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler22",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireRetryEventPublishingTest+FailingHandlerImpl, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler23",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireSerializeEventPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "TestHandler24",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.MultiEventPublishingTest+HangfireEventHandler, Teleopti.Ccc.InfrastructureTest"
			};


			yield return new MappingSpec
			{
				CurrentPersistedName = "HangfireJobFailuresTest+TestHandler",
				CurrentTypeName = "Teleopti.Wfm.Administration.IntegrationTest.Hangfire.HangfireJobFailuresTest+TestHandler, Teleopti.Wfm.Administration.IntegrationTest"
			};
			yield return new MappingSpec
			{
				CurrentPersistedName = "HangfireJobPerformanceTest+TestHandler",
				CurrentTypeName = "Teleopti.Wfm.Administration.IntegrationTest.Hangfire.HangfireJobPerformanceTest+TestHandler, Teleopti.Wfm.Administration.IntegrationTest"
			};
		}

		protected class MappingSpec
		{
			public string CurrentPersistedName;
			public IEnumerable<string> LegacyPersistedNames = Enumerable.Empty<string>();

			public string CurrentTypeName;
		}
	}
}