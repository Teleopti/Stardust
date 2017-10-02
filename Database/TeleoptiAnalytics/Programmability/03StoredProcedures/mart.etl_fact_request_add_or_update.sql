IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_request_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_request_add_or_update]
GO
-- =============================================
-- Description:	add or update fact_request
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_request_add_or_update] 
	   @request_code uniqueidentifier
      ,@person_id int
      ,@request_start_date_id int
      ,@application_datetime smalldatetime
      ,@request_startdate smalldatetime
      ,@request_enddate smalldatetime
      ,@request_type_id tinyint
      ,@request_status_id tinyint
      ,@request_day_count int
      ,@request_start_date_count int
      ,@business_unit_id smallint
      ,@datasource_update_date smalldatetime
      ,@absence_id int
      ,@request_starttime smalldatetime
      ,@request_endtime smalldatetime
      ,@requested_time_m int
AS

MERGE mart.fact_request AS target  
    USING (SELECT 
				@request_code
				,@person_id
				,@request_start_date_id
				,@application_datetime
				,@request_startdate
				,@request_enddate
				,@request_type_id
				,@request_status_id
				,@request_day_count
				,@request_start_date_count
				,@business_unit_id
				,@datasource_update_date
				,@absence_id
				,@request_starttime
				,@request_endtime
				,@requested_time_m)	AS src (request_code,
											person_id,
											request_start_date_id,
											application_datetime,
											request_startdate,
											request_enddate,
											request_type_id,
											request_status_id,
											request_day_count,
											request_start_date_count,
											business_unit_id,
											datasource_update_date,
											absence_id,
											request_starttime,
											request_endtime,
											requested_time_m)
		ON (target.request_code = src.request_code)  
    WHEN MATCHED THEN   
        UPDATE set target.person_id =src.person_id
		,request_start_date_id = src.request_start_date_id
		,application_datetime = src.application_datetime
		,request_startdate = src.request_startdate
		,request_enddate = src.request_enddate
		,request_type_id = src.request_type_id
		,request_status_id = src.request_status_id
		,request_day_count = src.request_day_count
		,request_start_date_count = src.request_start_date_count
		,business_unit_id = src.business_unit_id
		,datasource_id = 1
		,update_date = GETUTCDATE()
		,datasource_update_date = src.datasource_update_date
		,absence_id = src.absence_id
		,request_starttime = src.request_starttime
		,request_endtime = src.request_endtime
		,requested_time_m = src.requested_time_m
WHEN NOT MATCHED THEN  
    INSERT (request_code ,
	person_id,
	request_start_date_id,
	application_datetime,
	request_startdate,
	request_enddate,
	request_type_id,
	request_status_id,
	request_day_count,
	request_start_date_count,
	business_unit_id,
	datasource_id,
	insert_date,
	update_date,
	datasource_update_date,
	absence_id,
	request_starttime,
	request_endtime,
	requested_time_m)
    VALUES (src.request_code
	,src.person_id
	,src.request_start_date_id
	,src.application_datetime
	,src.request_startdate
	,src.request_enddate
	,src.request_type_id
	,src.request_status_id
	,src.request_day_count
	,src.request_start_date_count
	,src.business_unit_id
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,src.datasource_update_date
	,src.absence_id
	,src.request_starttime
	,src.request_endtime
	,src.requested_time_m);

GO