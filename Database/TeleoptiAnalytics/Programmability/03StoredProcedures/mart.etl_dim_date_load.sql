IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_date_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_date_load]
GO


-- =============================================
-- Author:		ChLu
-- Description:	Loads date from stg_date to dim_date.
-- Create date: 2008-01-30
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-04-27 Change min/maxdate format DaJo
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_date_load] 
	
WITH EXECUTE AS OWNER
AS
--Create min and max date
DECLARE @mindate as smalldatetime, @maxdate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime), @maxdate=CAST('20591231' as smalldatetime)

--------------------------------------------------------------------------
-- Not Defined date and Eternity date
SET IDENTITY_INSERT mart.dim_date ON

INSERT INTO mart.dim_date
	(
	date_id, 
	date_date, 
	year, 
	year_month, 
	month, 
	month_name, 
	day_in_month, 
	weekday_number, 
	weekday_name, 
	week_number, 
	year_week, 
	quarter
	)
SELECT 
	date_id			= -1, 
	date_date		= @mindate, 
	year			= '0000', 
	year_month		= '000000', 
	month			= '00', 
	month_name		= 'Not Defined', 
	day_in_month	= '0', 
	weekday_number	= '0', 
	weekday_name	= 'Not Defined', 
	week_number		= '0', 
	year_week		= '000000', 
	quarter			= 'ND'
WHERE NOT EXISTS (SELECT * FROM mart.dim_date where date_id = -1)

INSERT INTO mart.dim_date
	(
	date_id, 
	date_date, 
	year, 
	year_month, 
	month, 
	month_name, 
	day_in_month, 
	weekday_number, 
	weekday_name, 
	week_number, 
	year_week, 
	quarter
	)
SELECT 
	date_id			= -2, 
	date_date		= @maxdate, 
	year			= '0000', 
	year_month		= '000000', 
	month			= '00', 
	month_name		= 'Eternity', 
	day_in_month	= '0', 
	weekday_number	= '0', 
	weekday_name	= 'Eternity', 
	week_number		= '0', 
	year_week		= '000000', 
	quarter			= 'ET'
WHERE NOT EXISTS (SELECT * FROM mart.dim_date where date_id = -2)

SET IDENTITY_INSERT mart.dim_date OFF

--update incorrect date attributes
UPDATE mart.dim_date
SET year				= s.year, 
	year_month			= s.year_month, 
	month				= s.month, 
	month_name			= s.month_name, 
	month_resource_key	= s.month_resource_key,
	day_in_month		= s.day_in_month, 
	weekday_number		= s.weekday_number, 
	weekday_name		= s.weekday_name, 
	weekday_resource_key= s.weekday_resource_key,
	week_number			= s.week_number, 
	year_week			= s.year_week, 
	quarter				= s.quarter,
	insert_date			 = getdate()
FROM
	Stage.stg_date s
INNER JOIN mart.dim_date d ON s.date_date = d.date_date

----------------------------------------------------------------------------
-- Insert new dates
INSERT INTO mart.dim_date
	(
	date_date, 
	year, 
	year_month, 
	month, 
	month_name, 
	month_resource_key,
	day_in_month, 
	weekday_number, 
	weekday_name, 
	weekday_resource_key,
	week_number, 
	year_week, 
	quarter
	)
SELECT 
	date_date			= s.date_date, 
	year				= s.year, 
	year_month			= s.year_month, 
	month				= s.month, 
	month_name			= s.month_name, 
	month_resource_key	= s.month_resource_key,
	day_in_month		= s.day_in_month, 
	weekday_number		= s.weekday_number, 
	weekday_name		= s.weekday_name, 
	weekday_resource_key= s.weekday_resource_key,
	week_number			= s.week_number, 
	year_week			= s.year_week, 
	quarter				= s.quarter
FROM
	Stage.stg_date s
WHERE 
	NOT EXISTS (SELECT date_id FROM mart.dim_date d WHERE d.date_date = s.date_date)

GO

