IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_requested_day_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_requested_day_add_or_update]
GO
-- =============================================
-- Description:	add or update fact_requested_day
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_requested_day_add_or_update] 
	   @request_code uniqueidentifier
      ,@person_id int
      ,@request_date_id int
      ,@request_type_id tinyint
      ,@request_status_id tinyint
      ,@request_day_count int
      ,@business_unit_id smallint
      ,@datasource_update_date smalldatetime
      ,@absence_id int
AS
declare @rows int

INSERT INTO mart.fact_requested_days
SELECT 
	@request_code
	,@person_id
	,@request_date_id
	,@request_type_id
	,@request_status_id
	,@request_day_count
	,@business_unit_id
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,@datasource_update_date
	,@absence_id
WHERE NOT EXISTS(SELECT 1 FROM mart.fact_requested_days WHERE request_code=@request_code AND request_date_id=@request_date_id)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.fact_requested_days
	SET
		 person_id = @person_id
		,request_type_id = @request_type_id
		,request_status_id = @request_status_id
		,request_day_count = @request_day_count
		,business_unit_id = @business_unit_id
		,datasource_id = 1
		,update_date = GETUTCDATE()
		,datasource_update_date = @datasource_update_date
		,absence_id = @absence_id
	WHERE
	request_code = @request_code AND request_date_id=@request_date_id
END
GO
