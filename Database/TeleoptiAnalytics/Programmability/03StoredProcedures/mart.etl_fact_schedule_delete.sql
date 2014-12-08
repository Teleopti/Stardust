IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_delete]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2014-12-08
-- Description:	Delete all schedule rows for shift starting on given date id
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_delete]
	@shift_startdate_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	DELETE FROM mart.fact_schedule
	WHERE shift_startdate_id = @shift_startdate_id
END

GO


