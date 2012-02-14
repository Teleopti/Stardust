IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_time_zone]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_time_zone]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-08-05
-- Update date: 2009-02-11
--				20080910 Added parameter @bu_id KJ
--				20090211 Added new mart schema KJ
--				20090302 Excluded UTC-timezone from load KJ
--				2012-01-24 Trying to hide the time zone for ReportId=27 (request per Agent)
-- Description:	Loads time zones to report control cboTimeZone
-- =============================================
CREATE PROCEDURE [mart].[report_control_time_zone] 
@report_id int,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS


--hide time zone in this report(s)
IF @report_id in (27)
	SELECT 1 as id ,'DontDisplayMe' as name  --because im the only time zone
ELSE
	SELECT 
		tz.time_zone_id as id,
		tz.time_zone_name as name
	FROM mart.dim_time_zone tz
	WHERE tz.time_zone_code<>'UTC' --ta inte med UTC
	ORDER BY name

GO

