IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_request_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_request_delete]
GO
-- =============================================
-- Description:	delete fact_request and fact_requested_day
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_request_delete] 
	   @request_code uniqueidentifier
AS
BEGIN

DELETE FROM mart.fact_requested_days
WHERE request_code=@request_code

DELETE FROM mart.fact_request
WHERE request_code=@request_code

END
GO
