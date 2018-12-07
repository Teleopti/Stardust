using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsRequestRepository : IAnalyticsRequestRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;

		public AnalyticsRequestRepository(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork)
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
		}

		public void AddOrUpdate(AnalyticsRequest analyticsRequest)
		{
			_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				mart.etl_fact_request_add_or_update 
				   @request_code=:{nameof(analyticsRequest.RequestCode)}
				  ,@person_id=:{nameof(analyticsRequest.PersonId)}
				  ,@request_start_date_id=:{nameof(analyticsRequest.RequestStartDateId)}
				  ,@application_datetime=:{nameof(analyticsRequest.ApplicationDatetime)}
				  ,@request_startdate=:{nameof(analyticsRequest.RequestStartDate)}
				  ,@request_enddate=:{nameof(analyticsRequest.RequestEndDate)}
				  ,@request_type_id=:{nameof(analyticsRequest.RequestTypeId)}
				  ,@request_status_id=:{nameof(analyticsRequest.RequestStatusId)}
				  ,@request_day_count=:{nameof(analyticsRequest.RequestDayCount)}
				  ,@request_start_date_count=:{nameof(analyticsRequest.RequestStartDateCount)}
				  ,@business_unit_id=:{nameof(analyticsRequest.BusinessUnitId)}
				  ,@datasource_update_date=:{nameof(analyticsRequest.DatasourceUpdateDate)}
				  ,@absence_id=:{nameof(analyticsRequest.AbsenceId)}
				  ,@request_starttime=:{nameof(analyticsRequest.RequestStartTime)}
				  ,@request_endtime=:{nameof(analyticsRequest.RequestEndTime)}
				  ,@requested_time_m=:{nameof(analyticsRequest.RequestedTimeMinutes)}")
				.SetParameter(nameof(analyticsRequest.RequestCode), analyticsRequest.RequestCode)
				.SetParameter(nameof(analyticsRequest.PersonId), analyticsRequest.PersonId)
				.SetParameter(nameof(analyticsRequest.RequestStartDateId), analyticsRequest.RequestStartDateId)
				.SetParameter(nameof(analyticsRequest.ApplicationDatetime), analyticsRequest.ApplicationDatetime)
				.SetParameter(nameof(analyticsRequest.RequestStartDate), analyticsRequest.RequestStartDate)
				.SetParameter(nameof(analyticsRequest.RequestEndDate), analyticsRequest.RequestEndDate)
				.SetParameter(nameof(analyticsRequest.RequestTypeId), analyticsRequest.RequestTypeId)
				.SetParameter(nameof(analyticsRequest.RequestStatusId), analyticsRequest.RequestStatusId)
				.SetParameter(nameof(analyticsRequest.RequestDayCount), analyticsRequest.RequestDayCount)
				.SetParameter(nameof(analyticsRequest.RequestStartDateCount), analyticsRequest.RequestStartDateCount)
				.SetParameter(nameof(analyticsRequest.BusinessUnitId), analyticsRequest.BusinessUnitId)
				.SetParameter(nameof(analyticsRequest.DatasourceUpdateDate), analyticsRequest.DatasourceUpdateDate)
				.SetParameter(nameof(analyticsRequest.AbsenceId), analyticsRequest.AbsenceId)
				.SetParameter(nameof(analyticsRequest.RequestStartTime), analyticsRequest.RequestStartTime)
				.SetParameter(nameof(analyticsRequest.RequestEndTime), analyticsRequest.RequestEndTime)
				.SetParameter(nameof(analyticsRequest.RequestedTimeMinutes), analyticsRequest.RequestedTimeMinutes)
				.ExecuteUpdate();
		}

		public void AddOrUpdate(AnalyticsRequestedDay analyticsRequestedDay)
		{
			_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				mart.etl_fact_requested_day_add_or_update 
				   @request_code=:{nameof(analyticsRequestedDay.RequestCode)}
				  ,@person_id=:{nameof(analyticsRequestedDay.PersonId)}
				  ,@request_date_id=:{nameof(analyticsRequestedDay.RequestDateId)}
				  ,@request_type_id=:{nameof(analyticsRequestedDay.RequestTypeId)}
				  ,@request_status_id=:{nameof(analyticsRequestedDay.RequestStatusId)}
				  ,@request_day_count=:{nameof(analyticsRequestedDay.RequestDayCount)}
				  ,@business_unit_id=:{nameof(analyticsRequestedDay.BusinessUnitId)}
				  ,@datasource_update_date=:{nameof(analyticsRequestedDay.DatasourceUpdateDate)}
				  ,@absence_id=:{nameof(analyticsRequestedDay.AbsenceId)}")
				.SetParameter(nameof(analyticsRequestedDay.RequestCode), analyticsRequestedDay.RequestCode)
				.SetParameter(nameof(analyticsRequestedDay.PersonId), analyticsRequestedDay.PersonId)
				.SetParameter(nameof(analyticsRequestedDay.RequestDateId), analyticsRequestedDay.RequestDateId)
				.SetParameter(nameof(analyticsRequestedDay.RequestTypeId), analyticsRequestedDay.RequestTypeId)
				.SetParameter(nameof(analyticsRequestedDay.RequestStatusId), analyticsRequestedDay.RequestStatusId)
				.SetParameter(nameof(analyticsRequestedDay.RequestDayCount), analyticsRequestedDay.RequestDayCount)
				.SetParameter(nameof(analyticsRequestedDay.BusinessUnitId), analyticsRequestedDay.BusinessUnitId)
				.SetParameter(nameof(analyticsRequestedDay.DatasourceUpdateDate), analyticsRequestedDay.DatasourceUpdateDate)
				.SetParameter(nameof(analyticsRequestedDay.AbsenceId), analyticsRequestedDay.AbsenceId)
				.ExecuteUpdate();
		}

		public IList<AnalyticsRequestedDay> GetAnalyticsRequestedDays(Guid requestId)
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				  SELECT 
					   [request_code] {nameof(AnalyticsRequestedDay.RequestCode)}
					  ,[person_id] {nameof(AnalyticsRequestedDay.PersonId)}
					  ,[request_date_id] {nameof(AnalyticsRequestedDay.RequestDateId)}
					  ,[request_type_id] {nameof(AnalyticsRequestedDay.RequestTypeId)}
					  ,[request_status_id] {nameof(AnalyticsRequestedDay.RequestStatusId)}
					  ,[request_day_count] {nameof(AnalyticsRequestedDay.RequestDayCount)}
					  ,[business_unit_id] {nameof(AnalyticsRequestedDay.BusinessUnitId)}
					  ,[datasource_id] {nameof(AnalyticsRequestedDay.DatasourceId)}
					  ,[insert_date] {nameof(AnalyticsRequestedDay.InsertDate)}
					  ,[update_date] {nameof(AnalyticsRequestedDay.UpdateDate)}
					  ,[datasource_update_date] {nameof(AnalyticsRequestedDay.DatasourceUpdateDate)}
					  ,[absence_id] {nameof(AnalyticsRequestedDay.AbsenceId)}
				  FROM [mart].[fact_requested_days] WITH (NOLOCK)
				  WHERE request_code=:{nameof(requestId)}")
				.SetParameter(nameof(requestId), requestId)
				.SetResultTransformer(Transformers.AliasToBean<AnalyticsRequestedDay>())
				.List<AnalyticsRequestedDay>();
		}

		public void Delete(IEnumerable<AnalyticsRequestedDay> analyticsRequestedDays)
		{
			foreach (var analyticsRequestedDay in analyticsRequestedDays)
			{
				_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
					mart.etl_fact_requested_day_delete
					   @request_code=:{nameof(analyticsRequestedDay.RequestCode)}
					  ,@request_date_id=:{nameof(analyticsRequestedDay.RequestDateId)}")
				.SetParameter(nameof(analyticsRequestedDay.RequestCode), analyticsRequestedDay.RequestCode)
				.SetParameter(nameof(analyticsRequestedDay.RequestDateId), analyticsRequestedDay.RequestDateId)
				.ExecuteUpdate();
			}
		}

		public void Delete(Guid requestId)
		{
			_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
					mart.etl_fact_request_delete
					   @request_code=:{nameof(requestId)}")
				.SetParameter(nameof(requestId), requestId)
				.ExecuteUpdate();
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			_currentAnalyticsUnitOfWork.Current()
				.Session()
				.CreateSQLQuery(
					$@"exec mart.etl_fact_request_update_unlinked_personids 
							@person_periodids=:PersonIds
							")
				.SetString("PersonIds", string.Join(",", personPeriodIds))
				.ExecuteUpdate();
		}
	}
}