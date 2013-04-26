IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_intraday_load]
GO

-- =============================================
-- Author:		David J
-- Create date: 2013-04-26
-- Description:	Write schedule preferences from staging table 'stg_schedule_preference'
--				to data mart table 'fact_schedule_preference'. Used only by ETL.Intrday
-- =============================================
--exec mart.etl_fact_schedule_preference_intraday_load '493E828B-D416-4628-AB10-990E7D268DB9','493E828B-D416-4628-AB10-990E7D268DB9'

CREATE PROCEDURE [mart].[etl_fact_schedule_preference_intraday_load]
@business_unit_code uniqueidentifier,
@scenario_code uniqueidentifier
AS
SET NOCOUNT ON

if (select count(*)
	from mart.dim_scenario
	where business_unit_code = @business_unit_code
	and scenario_code = @scenario_code
	) <> 1
BEGIN
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = 'This is not a default scenario, or muliple default scenarios exists!'
	RAISERROR (@ErrorMsg,16,1)
	RETURN 0
END



RETURN 0
GO
