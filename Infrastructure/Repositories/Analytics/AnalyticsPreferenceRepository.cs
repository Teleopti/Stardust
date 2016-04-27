using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPreferenceRepository : IAnalyticsPreferenceRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsPreferenceRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public void AddPreference(AnalyticsFactSchedulePreference analyticsFactSchedulePreference)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				var insertAndUpdateDateTime = DateTime.Now;
				var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_fact_schedule_preference_insert]
					   @date_id=:DateId
					  ,@interval_id=:IntervalId
					  ,@person_id=:PersonId
					  ,@scenario_id=:ScenarioId
					  ,@preference_type_id=:PreferenceTypeId
					  ,@shift_category_id=:ShiftCategoryId
					  ,@day_off_id=:DayOffId
					  ,@preferences_requested=:PreferencesRequested
					  ,@preferences_fulfilled=:PreferencesFulfilled
					  ,@preferences_unfulfilled=:PreferencesUnfulfilled
					  ,@business_unit_id=:BusinessUnitId
					  ,@datasource_id=:DatasourceId
					  ,@insert_date=:InsertDate
					  ,@update_date=:UpdateDate
					  ,@datasource_update_date=:DatasourceUpdateDate
					  ,@must_haves=:MustHaves
					  ,@absence_id=:AbsenceId")
					.SetInt32("DateId", analyticsFactSchedulePreference.DateId)
					.SetInt32("IntervalId", analyticsFactSchedulePreference.IntervalId)
					.SetInt32("PersonId", analyticsFactSchedulePreference.PersonId)
					.SetInt32("ScenarioId", analyticsFactSchedulePreference.ScenarioId)
					.SetInt32("PreferenceTypeId", analyticsFactSchedulePreference.PreferenceTypeId)
					.SetInt32("ShiftCategoryId", analyticsFactSchedulePreference.ShiftCategoryId)
					.SetInt32("DayOffId", analyticsFactSchedulePreference.DayOffId)
					.SetInt32("PreferencesRequested", analyticsFactSchedulePreference.PreferencesRequested)
					.SetInt32("PreferencesFulfilled", analyticsFactSchedulePreference.PreferencesFulfilled)
					.SetInt32("PreferencesUnfulfilled", analyticsFactSchedulePreference.PreferencesUnfulfilled)
					.SetInt32("BusinessUnitId", analyticsFactSchedulePreference.BusinessUnitId)
					.SetInt32("DatasourceId", analyticsFactSchedulePreference.DatasourceId)
					.SetDateTime("InsertDate", insertAndUpdateDateTime)
					.SetDateTime("UpdateDate", insertAndUpdateDateTime)
					.SetDateTime("DatasourceUpdateDate", analyticsFactSchedulePreference.DatasourceUpdateDate)
					.SetInt32("MustHaves", analyticsFactSchedulePreference.MustHaves)
					.SetInt32("AbsenceId", analyticsFactSchedulePreference.AbsenceId);
				query.ExecuteUpdate();
			}
		}

		public void DeletePreferences(int dateId, int personId, int? scenarioId = null)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_fact_schedule_preference_delete]
					   @date_id=:DateId
					  ,@person_id=:PersonId
					  ,@scenario_id=:ScenarioId")
					.SetInt32("DateId", dateId)
					.SetInt32("PersonId", personId);

				if (scenarioId.HasValue)
				{
					query.SetInt32("ScenarioId", scenarioId.Value);
				}
				else
				{
					query.SetParameter("ScenarioId", null, NHibernateUtil.Int32);
				}
				query.ExecuteUpdate();
			}
		}

		public IList<AnalyticsFactSchedulePreference> PreferencesForPerson(int personId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					@"SELECT [date_id] DateId
						  ,[interval_id] IntervalId
						  ,[person_id] PersonId
						  ,[scenario_id] ScenarioId
						  ,[preference_type_id] PreferenceTypeId
						  ,[shift_category_id] ShiftCategoryId
						  ,[day_off_id] DayOffId
						  ,[preferences_requested] PreferencesRequested
						  ,[preferences_fulfilled] PreferencesFulfilled
						  ,[preferences_unfulfilled] PreferencesUnfulfilled
						  ,[business_unit_id] BusinessUnitId
						  ,[datasource_id] DatasourceId
						  ,[insert_date] InsertDate
						  ,[update_date] UpdateDate
						  ,[datasource_update_date] DatasourceUpdateDate
						  ,[must_haves] MustHaves
						  ,[absence_id] AbsenceId
					  FROM [mart].[fact_schedule_preference] WITH (NOLOCK) WHERE person_id=:PersonId ")
					.SetInt32("PersonId", personId)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsFactSchedulePreference)))
					.SetReadOnly(true)
					.List<AnalyticsFactSchedulePreference>();
			}
		}
	}
}
