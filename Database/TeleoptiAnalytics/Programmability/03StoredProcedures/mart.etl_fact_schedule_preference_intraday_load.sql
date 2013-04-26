IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_intraday_load]
GO

-- =============================================
-- Author:		David J
-- Create date: 2008-11-19
-- Description:	Write schedule preferences from staging table 'stg_schedule_preference'
--				to data mart table 'fact_schedule_preference'.
-- Updates:		2009-01-16
--				2009-02-09 Stage moved to mart db, removed view KJ
--				2009-01-16 Changed fields in stg table KJ
--				2008-12-01 Changed Delete statement for multi BU. KJ
--				2009-12-09 Some intermediate hardcoded stuff on day_off_id, Henry Greijer and Jonas Nordh.
--				2010-10-12 #12055 - ETL - cant load preferences
--				2011-09-27 Fix start/end times = 0
--				2012-11-25 #19854 - PBI to add Shortname for DayOff.
-- Interface:	smalldatetime, with only datepart! No time allowed
-- =============================================
--exec mart.etl_fact_schedule_preference_intraday_load '493E828B-D416-4628-AB10-990E7D268DB9'

CREATE PROCEDURE [mart].[etl_fact_schedule_preference_intraday_load]
@business_unit_code uniqueidentifier,
@scenario_code uniqueidentifier
AS
SET NOCOUNT ON
CREATE TABLE #fakeCount (Col1 int)
DECLARE @count int
SET @count = 0
WHILE @count < 1000
BEGIN
	INSERT INTO #fakeCount (Col1)
	VALUES(@count)

	SELECT @count = @count +1
END
DELETE FROM #fakeCount

RETURN 0
GO
