IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduling_metrics_per_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduling_metrics_per_period]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		DJ
-- Create date: 2013-09-30
-- Description:	Get avrage figures for scheduling
-- =============================================

CREATE PROCEDURE [mart].[report_data_scheduling_metrics_per_period]
@scheduling_type_id int,
@date_from datetime,
@date_to datetime,
@interval_type int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;
--exec mart.report_data_scheduling_metrics_per_period @scheduling_type_id=N'1',@date_from='2013-09-09 00:00:00',@date_to='2013-09-30 00:00:00',@interval_type=N'7',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id='F7F3AF97-EC24-4EA8-A2C7-5175879C7ACC',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE TABLE #RESULT(period nvarchar(30),
					interval_type int,
					avg_exec_sec int,
					tot_exec_sec int,
					avg_skill_days int,
					tot_skill_days int,
					avg_agents int,
					tot_agents int,
					tot_schedule int,
					block_schedule int,
					team_scheduling int
					)

INSERT INTO #result(period,interval_type,avg_exec_sec,tot_exec_sec, avg_skill_days, tot_skill_days,avg_agents,tot_agents,tot_schedule,block_schedule, team_scheduling)
SELECT	'2013-09-02',1,20,400,13,130,10,200,30,2,11


INSERT INTO #result(period,interval_type,avg_exec_sec,tot_exec_sec, avg_skill_days, tot_skill_days,avg_agents,tot_agents,tot_schedule,block_schedule, team_scheduling)
SELECT	'2013-09-03',1,24,593,12,260,12,230,26,7,8


/* detta behöver man för att gruppera på period typer
SELECT 
	CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END AS 'period',

select alla andra värden


GROUP BY
	CASE @interval_type 
	WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END

ORDER BY 
		CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
		END




	*/

SELECT *  FROM #result


END