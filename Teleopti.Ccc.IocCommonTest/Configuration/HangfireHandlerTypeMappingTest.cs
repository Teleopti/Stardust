using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class HangfireHandlerTypeMappingTest
	{
		public HandlerTypeMapperForTest Mapper;

		[Test]
		public void AllNewHandlersShouldBeMapped()
		{
			var hangfireHandlers =
				from assembly in EventHandlerLocations.Assemblies()
				from type in assembly.GetTypes()
				where type.IsEventHandler()
				where type.RunsOnHangfire()
				select type;

			var exceptions = hangfireHandlers
				.Select(x =>
				{
					try
					{
						Mapper.NameForPersistence(x);
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

		[Test, Ignore("WIP")]
		public void AllMovedOrRemovedOrRenamedHandlersShouldBeConsidered()
		{
			Mapper.AllCurrentTypeNames()
				.Select(x => Type.GetType(x, true))
				.ForEach(x => x.Should().Not.Be.Null());
		}

		[Test]
		public void AllKnownPersistedNames20181210ShouldWork()
		{
			var legacyTypes = new[]
			{
				"Teleopti.Ccc.Domain.Staffing.SendUpdateStaffingReadModelHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.PublishInitializeReadModelEventHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.RequeueHangfireEventHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.Infrastructure.CleanFailedQueueHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonEmploymentChangedEventHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.TerminatePersonHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ReadModelInitializeHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonAssociationChangedEventPublisher, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.HandlerDisabledByTestToggle, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.HandlerMethodDisabledByTestToggle, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PackageHandlerEnabledByTestToggle, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.SameHandlerEnabledByTestToggle, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.UpdateFindPersonDataHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.BuildInGroupsAnalyticsUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers.GroupingReadModelDataUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Absence.AnalyticsAbsenceUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Scenario.AnalyticsScenarioUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Preference.PreferenceFulfillmentChangedHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Preference.AnalyticsPreferenceUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.AnalyticsGroupPageUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers.GroupingReadModelGroupPageUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers.AnalyticsOvertimeUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.QuickForecastWorkloadsEventHandlerHangfire, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Forecast.RecalculateForecastOnSkillEventHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.DayOff.AnalyticsDayOffUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Activity.AnalyticsActivityUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Workload.AnalyticsWorkloadUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.SkillDay.AnalyticsForecastWorkloadUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData.AnalyticsPersonNameUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit.AnalyticsBusinessUnitUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsSiteUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers.AnalyticsTeamUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsPersonLocalDateUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Dates.AnalyticsDateChangedHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsTimeZoneUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsHourlyAvailabilityMatchingPerson, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsOptionalColumnGroupPageHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPreferenceMatchingPerson, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsRequestMatchingPerson, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodSkillsUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.PersonFinderReadOnlyUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.UpdateGroupingReadModelHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Skill.AnalyticsSkillUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.ShiftCategorySelectionModelUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers.AnalyticsShiftCategoryUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade.ShiftTradeRequestHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedInDefaultScenarioNotificationNew, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedNotifierHangfire, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ProjectionChangedEventPublisher, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection.ScheduleProjectionReadOnlyUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleReadModelWrapperHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ShiftExchangeOfferHandlerNew, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel.ScheduleDayReadModelHandlerHangfire, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel.PersonScheduleDayReadModelUpdaterHangfire, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Availability.AnalyticsAvailabilityUpdater, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.Audit.PurgeAuditHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.WaitlistProcessPublisher, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.AbsenceRequestQueueStrategyHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.RequestPersonAbsenceRemovedEventHandler, Teleopti.Ccc.Domain",
				"Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.NewAbsenceReport, Teleopti.Ccc.Domain",
				"Teleopti.Wfm.Adherence.Domain.Service.AgentStateMaintainer, Teleopti.Wfm.Adherence",
				"Teleopti.Wfm.Adherence.Domain.Service.ScheduleChangeProcessor, Teleopti.Wfm.Adherence",
				"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.AgentStateReadModelMaintainer, Teleopti.Wfm.Adherence",
				"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.ExternalLogonReadModelUpdater, Teleopti.Wfm.Adherence",
				"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.HistoricalOverviewReadModelMaintainer, Teleopti.Wfm.Adherence",
				"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.MappingReadModelUpdater, Teleopti.Wfm.Adherence",
				"Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels.RtaEventStoreMaintainer, Teleopti.Wfm.Adherence",
			};

			legacyTypes.ForEach(x => { Mapper.TypeForPersistedName(x); });
		}
	}
}