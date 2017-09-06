/****** Object:  StoredProcedure [mart].[report_data_forecast_vs_scheduled_hours]    Script Date: 10/09/2008 14:21:03 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_forecast_vs_scheduled_hours]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_forecast_vs_scheduled_hours]
GO
/****** Object:  StoredProcedure [mart].[report_data_forecast_vs_scheduled_hours]    Script Date: 10/09/2008 14:21:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		KJ
-- Create date: 20080528
-- Description:	Used by report Forecast vs Scheduled Hours
-- Update date:	20080924 Changed relative_difference calcualtion, removed abs() value	
--				20080926 Added column relative_deiffernce_incl_shrinkage
--				20081009 Bug fix Week Formats changed KJ	
--				20081106 Changed check on weekday but this must be changed later KJ		
--              20090115 Added check on weekday_resource_key for correct weekday name KJ
--				20090211 Added new mart schema KJ	
--				20090302 Excluded timezone UTC from time_zone check KJ	
--				20090701 Chnaged use of weekdayname, now using resourcekey field KJ	
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
--exec report_data_forecast_vs_scheduled_hours @scenario_id=N'0',@skill_set=N'0,5,4',@date_from='2006-01-02 00:00:00:000',@date_to='2006-01-11 00:00:00:000',@interval_from=N'0',@interval_to=N'287',@time_zone_id=81,@person_code='10bbde88-ffc3-4f55-8396-9ab60024b7a9',@report_id=7,@language_id=1053

CREATE PROCEDURE [mart].[report_data_forecast_vs_scheduled_hours] 
@scenario_id int,
@skill_set nvarchar(max),
@date_from datetime,
@date_to datetime,
@interval_from int,
@interval_to int,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier

AS
SET NOCOUNT ON

/*SNABBA UPP PROCEDUR, HÃ„MTA RÃ„TT TIMEZONE FÃ–RST OCH JOINA PÃ… TEMPTABELL SEDAN*/
--SELECT * 
--INTO #bridge_time_zone
--FROM bridge_time_zone
--WHERE time_zone_id=@time_zone_id

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0


CREATE TABLE #skills(id int)
INSERT INTO #skills
SELECT * FROM mart.SplitStringInt(@skill_set)


SELECT	convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))'year_week',
		LEFT(convert(varchar(30),d.date_date,120),10) 'date_date',
		d.weekday_number,
		CASE d.weekday_resource_key 
		WHEN 'ResDayOfWeekMonday' THEN (select isnull(term_language,d.weekday_name) from mart.language_translation where resourcekey='ResMonday' and language_id=@language_id)
		WHEN 'ResDayOfWeekTuesday' THEN (select isnull(term_language,d.weekday_name) from mart.language_translation where resourcekey='ResTuesday' and language_id=@language_id)
		WHEN 'ResDayOfWeekWednesday' THEN (select isnull(term_language,d.weekday_name) from mart.language_translation where resourcekey='ResWednesday' and language_id=@language_id)
		WHEN 'ResDayOfWeekThursday' THEN (select isnull(term_language,d.weekday_name) from mart.language_translation where resourcekey='ResThursday' and language_id=@language_id)
		WHEN 'ResDayOfWeekFriday' THEN (select isnull(term_language,d.weekday_name) from mart.language_translation where resourcekey='ResFriday' and language_id=@language_id)
		WHEN 'ResDayOfWeekSaturday' THEN (select isnull(term_language,d.weekday_name) from mart.language_translation where resourcekey='ResSaturday' and language_id=@language_id)
		WHEN 'ResDayOfWeekSunday' THEN (select isnull(term_language,d.weekday_name) from mart.language_translation where resourcekey='ResSunday' and language_id=@language_id)
		END AS 'weekday_name',
		sk.skill_id,
		sk.skill_name,
		sum(forecasted_resources_m)'forecasted_agents_m',
		sum(forecasted_resources_incl_shrinkage_m)'forecasted_agents_incl_shrinkage_m',
		sum(scheduled_resources_m) 'scheduled_agents_m',
		CASE WHEN sum(forecasted_resources_m)= 0 THEN 0
		ELSE
			(sum(scheduled_resources_m)-sum(forecasted_resources_m)) /sum(forecasted_resources_m)
		END AS 'relative_difference',
		stdevp(relative_difference)'standard_deviation',
		@hide_time_zone as hide_time_zone,
		CASE WHEN sum(forecasted_resources_incl_shrinkage_m)= 0 THEN 0
		ELSE
			(sum(scheduled_resources_m)-sum(forecasted_resources_incl_shrinkage_m)) /sum(forecasted_resources_incl_shrinkage_m)
		END AS 'relative_difference_incl_shrinkage',
		sum(forecasted_tasks) AS 'forecasted_tasks',
		sum(estimated_tasks_answered_within_sl) AS 'estimated_tasks_answered_within_sl'
FROM mart.fact_schedule_forecast_skill f 
INNER JOIN mart.bridge_time_zone b
	ON	f.interval_id= b.interval_id
	AND f.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
INNER JOIN mart.dim_skill sk
	ON sk.skill_id=f.skill_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND f.scenario_id=@scenario_id
AND f.skill_id in (select id from #skills)
GROUP BY d.year_week,d.date_date,d.weekday_number,d.weekday_resource_key ,d.weekday_name,sk.skill_id,sk.skill_name
ORDER BY d.year_week,d.date_date,d.weekday_number,d.weekday_resource_key ,d.weekday_name,sk.skill_id,sk.skill_name


GO