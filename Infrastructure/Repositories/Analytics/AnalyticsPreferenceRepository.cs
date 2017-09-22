using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPreferenceRepository : IAnalyticsPreferenceRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsPreferenceRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void AddPreference(AnalyticsFactSchedulePreference analyticsFactSchedulePreference)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_fact_schedule_preference_insert]
					   @date_id=:{nameof(AnalyticsFactSchedulePreference.DateId)}
					  ,@interval_id=:{nameof(AnalyticsFactSchedulePreference.IntervalId)}
					  ,@person_id=:{nameof(AnalyticsFactSchedulePreference.PersonId)}
					  ,@scenario_id=:{nameof(AnalyticsFactSchedulePreference.ScenarioId)}
					  ,@preference_type_id=:{nameof(AnalyticsFactSchedulePreference.PreferenceTypeId)}
					  ,@shift_category_id=:{nameof(AnalyticsFactSchedulePreference.ShiftCategoryId)}
					  ,@day_off_id=:{nameof(AnalyticsFactSchedulePreference.DayOffId)}
					  ,@preferences_requested=:{nameof(AnalyticsFactSchedulePreference.PreferencesRequested)}
					  ,@preferences_fulfilled=:{nameof(AnalyticsFactSchedulePreference.PreferencesFulfilled)}
					  ,@preferences_unfulfilled=:{nameof(AnalyticsFactSchedulePreference.PreferencesUnfulfilled)}
					  ,@business_unit_id=:{nameof(AnalyticsFactSchedulePreference.BusinessUnitId)}
					  ,@datasource_id=:{nameof(AnalyticsFactSchedulePreference.DatasourceId)}
					  ,@insert_date=:{nameof(AnalyticsFactSchedulePreference.InsertDate)}
					  ,@update_date=:{nameof(AnalyticsFactSchedulePreference.UpdateDate)}
					  ,@datasource_update_date=:{nameof(AnalyticsFactSchedulePreference.DatasourceUpdateDate)}
					  ,@must_haves=:{nameof(AnalyticsFactSchedulePreference.MustHaves)}
					  ,@absence_id=:{nameof(AnalyticsFactSchedulePreference.AbsenceId)}")
				.SetInt32(nameof(AnalyticsFactSchedulePreference.DateId), analyticsFactSchedulePreference.DateId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.IntervalId), analyticsFactSchedulePreference.IntervalId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.PersonId), analyticsFactSchedulePreference.PersonId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.ScenarioId), analyticsFactSchedulePreference.ScenarioId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.PreferenceTypeId), analyticsFactSchedulePreference.PreferenceTypeId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.ShiftCategoryId), analyticsFactSchedulePreference.ShiftCategoryId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.DayOffId), analyticsFactSchedulePreference.DayOffId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.PreferencesRequested), analyticsFactSchedulePreference.PreferencesRequested)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.PreferencesFulfilled), analyticsFactSchedulePreference.PreferencesFulfilled)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.PreferencesUnfulfilled), analyticsFactSchedulePreference.PreferencesUnfulfilled)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.BusinessUnitId), analyticsFactSchedulePreference.BusinessUnitId)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.DatasourceId), analyticsFactSchedulePreference.DatasourceId)
				.SetDateTime(nameof(AnalyticsFactSchedulePreference.InsertDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsFactSchedulePreference.UpdateDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsFactSchedulePreference.DatasourceUpdateDate), analyticsFactSchedulePreference.DatasourceUpdateDate)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.MustHaves), analyticsFactSchedulePreference.MustHaves)
				.SetInt32(nameof(AnalyticsFactSchedulePreference.AbsenceId), analyticsFactSchedulePreference.AbsenceId);
			query.ExecuteUpdate();
		}

		public void DeletePreferences(int dateId, int personId, int? scenarioId = null)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_fact_schedule_preference_delete]
					   @date_id=:{nameof(dateId)}
					  ,@person_id=:{nameof(personId)}
					  ,@scenario_id=:{nameof(scenarioId)}")
				.SetInt32(nameof(dateId), dateId)
				.SetInt32(nameof(personId), personId);

			if (scenarioId.HasValue)
			{
				query.SetInt32(nameof(scenarioId), scenarioId.Value);
			}
			else
			{
				query.SetParameter(nameof(scenarioId), null, NHibernateUtil.Int32);
			}
			query.ExecuteUpdate();
		}

		public IList<AnalyticsFactSchedulePreference> PreferencesForPerson(int personId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT [date_id] {nameof(AnalyticsFactSchedulePreference.DateId)}
						  ,[interval_id] {nameof(AnalyticsFactSchedulePreference.IntervalId)}
						  ,[person_id] {nameof(AnalyticsFactSchedulePreference.PersonId)}
						  ,[scenario_id] {nameof(AnalyticsFactSchedulePreference.ScenarioId)}
						  ,[preference_type_id] {nameof(AnalyticsFactSchedulePreference.PreferenceTypeId)}
						  ,[shift_category_id] {nameof(AnalyticsFactSchedulePreference.ShiftCategoryId)}
						  ,[day_off_id] {nameof(AnalyticsFactSchedulePreference.DayOffId)}
						  ,[preferences_requested] {nameof(AnalyticsFactSchedulePreference.PreferencesRequested)}
						  ,[preferences_fulfilled] {nameof(AnalyticsFactSchedulePreference.PreferencesFulfilled)}
						  ,[preferences_unfulfilled] {nameof(AnalyticsFactSchedulePreference.PreferencesUnfulfilled)}
						  ,[business_unit_id] {nameof(AnalyticsFactSchedulePreference.BusinessUnitId)}
						  ,[datasource_id] {nameof(AnalyticsFactSchedulePreference.DatasourceId)}
						  ,[insert_date] {nameof(AnalyticsFactSchedulePreference.InsertDate)}
						  ,[update_date] {nameof(AnalyticsFactSchedulePreference.UpdateDate)}
						  ,[datasource_update_date] {nameof(AnalyticsFactSchedulePreference.DatasourceUpdateDate)}
						  ,[must_haves] {nameof(AnalyticsFactSchedulePreference.MustHaves)}
						  ,[absence_id] {nameof(AnalyticsFactSchedulePreference.AbsenceId)}
					  FROM [mart].[fact_schedule_preference] WITH (NOLOCK) WHERE person_id=:{nameof(personId)} ")
				.SetInt32(nameof(personId), personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsFactSchedulePreference)))
				.SetReadOnly(true)
				.List<AnalyticsFactSchedulePreference>();
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			_analyticsUnitOfWork.Current()
				.Session()
				.CreateSQLQuery(
					$@"exec mart.etl_fact_schedule_preference_update_unlinked_personids 
							@person_periodids=:PersonIds
							")
				.SetString("PersonIds", string.Join(",", personPeriodIds))
				.ExecuteUpdate();
		}
	}
}
