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
declare @rows int

INSERT INTO mart.fact_request
SELECT 
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
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,@datasource_update_date
	,@absence_id
	,@request_starttime
	,@request_endtime
	,@requested_time_m
WHERE NOT EXISTS(SELECT 1 FROM mart.fact_request WHERE request_code=@request_code)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.fact_request
	SET
		 person_id = @person_id
		,request_start_date_id = @request_start_date_id
		,application_datetime = @application_datetime
		,request_startdate = @request_startdate
		,request_enddate = @request_enddate
		,request_type_id = @request_type_id
		,request_status_id = @request_status_id
		,request_day_count = @request_day_count
		,request_start_date_count = @request_start_date_count
		,business_unit_id = @business_unit_id
		,datasource_id = 1
		,update_date = GETUTCDATE()
		,datasource_update_date = @datasource_update_date
		,absence_id = @absence_id
		,request_starttime = @request_starttime
		,request_endtime = @request_endtime
		,requested_time_m = @requested_time_m
	WHERE
	request_code = @request_code
END
GO
