/****** Object:  StoredProcedure [mart].[raptor_stat_agent]    Script Date: 09/01/2009 13:32:05 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_stat_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_stat_agent]
GO

/****** Object:  StoredProcedure [mart].[raptor_stat_agent]    Script Date: 09/01/2009 13:32:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:            AF
-- Create date: 2008-08-31
-- Update Date: 
-- Description:       Used by SDK and MyTime MyReport (the middle panel)
-- =============================================

CREATE PROCEDURE [mart].[raptor_stat_agent]
@scenario_code uniqueidentifier,
@date_from datetime,
@date_to datetime,
@person_code uniqueidentifier,
@time_zone_code nvarchar(200)
AS
SET NOCOUNT ON

DECLARE @time_zone_id int

SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

CREATE TABLE #fact_schedule (person_code uniqueidentifier, date smalldatetime, scheduled_ready_time_s INT)

INSERT #fact_schedule(person_code,date,scheduled_ready_time_s)
SELECT person_code,date_date,sum(scheduled_ready_time_m)*60
FROM mart.fact_schedule fs
INNER JOIN mart.dim_person p  WITH (NOLOCK) ON fs.person_id = p.person_id
INNER JOIN mart.bridge_time_zone b
           ON         fs.interval_id= b.interval_id
           AND fs.schedule_date_id= b.date_id
           AND b.time_zone_id = @time_zone_id
INNER JOIN mart.dim_date d 
           ON b.local_date_id = d.date_id
WHERE d.date_date BETWEEN  @date_from AND @date_to
AND (@date_from	between p.valid_from_date AND p.valid_to_date OR  @date_to	between p.valid_from_date AND p.valid_to_date)
AND p.person_code = @person_code
AND fs.scenario_id = (SELECT scenario_id from mart.dim_scenario where scenario_code = @scenario_code)
GROUP BY person_code,date_date

CREATE TABLE #fact_agent(
                                            date smalldatetime,
                                            person_code uniqueidentifier,
                                            logged_in_time_s int,
                                            not_ready_time_s int,
                                            ready_time_s int)

INSERT INTO #fact_agent(date,person_code,logged_in_time_s,not_ready_time_s,ready_time_s )
SELECT     d.date_date,
                      tricky.person_code,
                      sum(logged_in_time_s),
                      sum(not_ready_time_s),
                      sum(ready_time_s)
FROM mart.fact_agent fa
INNER JOIN mart.bridge_time_zone b
           ON         fa.interval_id= b.interval_id
           AND fa.date_id= b.date_id
           AND b.time_zone_id = @time_zone_id
INNER JOIN mart.dim_date d 
           ON b.local_date_id = d.date_id
INNER JOIN (SELECT DISTINCT ba.acd_login_id, p.person_code
                                 FROM mart.bridge_acd_login_person ba
                                 INNER JOIN mart.dim_person p WITH (NOLOCK)
                                            on p.person_id=ba.person_id
                                            AND (@date_from	between p.valid_from_date AND p.valid_to_date OR  @date_to	between p.valid_from_date AND p.valid_to_date)
                                            AND p.person_code = @person_code) tricky
                                 ON fa.acd_login_id = tricky.acd_login_id
WHERE d.date_date BETWEEN @date_from AND @date_to
GROUP BY tricky.person_code,d.date_date


SELECT  isnull(fa.person_code,fs.person_code) as person_code,
                      isnull(fa.date,fs.date) as date,
                      isnull(scheduled_ready_time_s,0) as scheduled_ready_time,
                      isnull(logged_in_time_s,0) as logged_in_time,
                      isnull(not_ready_time_s,0) as not_ready_time,
                      isnull(ready_time_s,0) as ready_time
FROM #fact_schedule fs
FULL OUTER JOIN #fact_agent fa ON fs.date = fa.date
                      AND fa.person_code = fs.person_code
ORDER BY person_code, date


GO


