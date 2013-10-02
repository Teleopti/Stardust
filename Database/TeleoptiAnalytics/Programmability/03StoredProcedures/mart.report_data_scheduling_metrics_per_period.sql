IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduling_metrics_per_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduling_metrics_per_period]
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
--exec mart.report_data_scheduling_metrics_per_period @scheduling_type_id=N'1',@date_from='2013-10-01 00:00:00',@date_to='2013-10-02 00:00:00',@interval_type=N'4',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id='F7F3AF97-EC24-4EA8-A2C7-5175879C7ACC',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE TABLE #RESULT(
	period nvarchar(30),
	interval_type int,
	avg_exec_sec numeric(10,2),
	tot_exec_sec int,
	avg_skill_days numeric(10,2),
	tot_skill_days int,
	avg_agents numeric(10,2),
	tot_agents int,
	tot_schedule int,
	block_schedule int,
	team_scheduling int
	)

create table #TempBlockOptionUsed(block_count int,LogDate datetime)
create table #TempTeamOptionUsed(team_count int,LogDate datetime)

insert into #TempBlockOptionUsed
select 1,logDate
from dbo.AdvancedLoggingService
where BlockOptions <> '(null)'
and  BlockOptions <> ''


insert into #TempTeamOptionUsed
select 1,logDate
from dbo.AdvancedLoggingService
where TeamOptions <> '(null)'
and  TeamOptions <> ''

INSERT INTO #result(period,interval_type,avg_exec_sec,tot_exec_sec, avg_skill_days, tot_skill_days,avg_agents,tot_agents,tot_schedule,block_schedule, team_scheduling)
SELECT
	period =
		CASE
			WHEN @interval_type <=4 THEN convert(varchar(10), m.LogDate, 120) --day
			WHEN @interval_type = 5 THEN 'Week ' + cast(datepart(week,m.LogDate) as varchar(2))
			WHEN @interval_type = 6 THEN convert(varchar(7), m.LogDate, 120) --month
			WHEN @interval_type = 7 THEN convert(varchar(4), m.LogDate, 120) + 'Q' + cast(datepart(quarter,m.LogDate) as char(1))
			ELSE						 convert(varchar(4), m.LogDate, 120)
		END,
	interval_type	= @interval_type,
	avg_exec_sec	= sum(ExecutionTime)/(sum(isnull(Agents,1)) * sum(isnull(SkillDays,1))),
	tot_exec_sec	= sum(ExecutionTime),
	avg_skill_days	= avg(SkillDays),
	tot_skill_days	= sum(SkillDays),
	avg_agents		= avg(Agents),
	tot_agents		= sum(Agents),
	tot_schedule	= count(m.LogDate),
	block_schedule	= sum(isnull(block_count,0)),
	team_scheduling	= sum(isnull(team_count,0))
	
FROM dbo.AdvancedLoggingService m
LEFT JOIN #TempTeamOptionUsed teamOptions
	ON teamOptions.LogDate = m.LogDate
LEFT JOIN #TempBlockOptionUsed blockOptions
	ON blockOptions.LogDate = m.LogDate
GROUP BY
CASE
	WHEN @interval_type <=4 THEN convert(varchar(10), m.LogDate, 120) --day
	WHEN @interval_type = 5 THEN 'Week ' +cast(datepart(week,m.LogDate) as varchar(2))
	WHEN @interval_type = 6 THEN convert(varchar(7), m.LogDate, 120) --month
	WHEN @interval_type = 7 THEN convert(varchar(4), m.LogDate, 120) + 'Q' + cast(datepart(quarter,m.LogDate) as char(1))
	ELSE						 convert(varchar(4), m.LogDate, 120)
END

SELECT * FROM #result
END