IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_time_zone]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_time_zone]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-08-05
-- Update date: 2009-02-11
--				2008-09-10 Added parameter @bu_id KJ
--				2009-02-11 Added new mart schema KJ
--				2009-03-02 Excluded UTC-timezone from load KJ
--				2012-01-24 Trying to hide the time zone for ReportId=27 (request per Agent)
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
--				2012-08-13 Added output column default_value to be able to preselect the default if no user setting. JN
-- Description:	Loads time zones to report control cboTimeZone
-- =============================================
-- exec [mart].[report_control_time_zone] '71BDB56D-C12F-489B-8275-04873A668D90', '10957AD5-5489-48E0-959A-9B5E015B2B5C', 1053, '10957AD5-5489-48E0-959A-9B5E015B2B5C'
CREATE PROCEDURE [mart].[report_control_time_zone] 
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS


--hide time zone in this report(s)
IF @report_id in ('8DE1AB0F-32C2-4619-A2B2-97385BE4C49C')
	SELECT 1 as id ,'DontDisplayMe' as name,  cast(0 as bit)  AS default_value  --because im the only time zone
ELSE
	DECLARE @is_utc_in_use bit
	SELECT @is_utc_in_use = utc_in_use FROM mart.dim_time_zone WHERE time_zone_code = 'UTC'
	
	IF @is_utc_in_use = 1
		SELECT 
			tz.time_zone_id as id,
			tz.time_zone_name as name,
			default_zone AS default_value
		FROM mart.dim_time_zone tz WITH(NOLOCK)
		WHERE to_be_deleted <> 1
		ORDER BY name
	ELSE
		SELECT 
			tz.time_zone_id as id,
			tz.time_zone_name as name,
			default_zone AS default_value
		FROM mart.dim_time_zone tz WITH(NOLOCK)
		WHERE tz.time_zone_code<>'UTC'
		and to_be_deleted <> 1
		ORDER BY name

GO

