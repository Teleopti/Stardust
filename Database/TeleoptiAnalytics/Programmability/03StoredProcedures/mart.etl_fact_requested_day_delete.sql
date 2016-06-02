IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_requested_day_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_requested_day_delete]
GO
-- =============================================
-- Description:	delete fact_requested_day
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_requested_day_delete] 
	   @request_code uniqueidentifier
      ,@request_date_id int
AS
BEGIN

DELETE FROM mart.fact_requested_days
WHERE request_code=@request_code AND request_date_id=@request_date_id

END
GO
