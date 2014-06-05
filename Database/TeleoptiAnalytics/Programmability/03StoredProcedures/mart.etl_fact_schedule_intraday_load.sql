IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_intraday_load]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2013-04-11
-- Description:	Write schedule activities from staging table 'stg_schedule'
--				to data mart table 'fact_schedule'.
-- Updates:
------------------------------------------------
--	Date		Who		Why
--	2013-10-25	DJ		#25126 - Remove unwanted UTC days from stg_schedule_updated_ShiftStartDateUTC
--	2013-10-25	DJ		#25309 - improve performance in mart.etl_fact_schedule_intraday_load
-- =============================================
--exec mart.etl_fact_schedule_intraday_load '2009-02-02','2009-02-03'
--exec mart.etl_fact_schedule_intraday_load '928DD0BC-BF40-412E-B970-9B5E015AADEA',1
/*
select 1
DBCC DROPCLEANBUFFERS --drop all data from SQL Server cache
DBCC FREEPROCCACHE --drop all pre-compiled query plans

SET STATISTICS IO ON --show I/O acitivity
SET STATISTICS TIME ON --show execution time
*/
CREATE PROCEDURE [mart].[etl_fact_schedule_intraday_load]
@business_unit_code uniqueidentifier,
@debug bit = 0 
WITH EXECUTE AS OWNER
AS

SET NOCOUNT ON

EXEC [mart].[stage_schedule_remove_overlapping_shifts]

--if no @scenario, no data then break
DECLARE @scenario_code uniqueidentifier
DECLARE @scenario_id smallint
SELECT TOP 1 @scenario_code=scenario_code FROM Stage.stg_schedule_changed
IF @scenario_code IS NULL
BEGIN
	RETURN 0
END

SELECT @scenario_id = scenario_id
FROM mart.dim_scenario
WHERE scenario_code=@scenario_code
AND default_scenario = 1

IF @scenario_id IS NULL
BEGIN
	RETURN 0
END

--debug
declare @timeStat table (step int,laststep_ms int,totalTime int)
declare @startTime datetime
declare @lastStep datetime
set @lastStep=getdate()
set @startTime=getdate()
declare @step int
set @step=0

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

--temp table for perf.
CREATE TABLE #stg_schedule(
	[scenario_id] [smallint] NOT NULL,
	[interval_id] [int] NOT NULL,
	[date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL
)

CREATE TABLE #stg_schedule_changed(
	[person_id] [int] NOT NULL,
	[shift_startdate_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL
)

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

--Get first row scenario in stage table, currently this must(!) be the default scenario, else RAISERROR
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

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

--prepare a temp table for better performance on delete
INSERT INTO #stg_schedule_changed
select DISTINCT
	f.person_id,
	f.shift_startdate_id,
	f.scenario_id
from mart.fact_schedule f
inner join mart.dim_person p
	on p.person_id = f.person_id
inner join mart.bridge_time_zone btz
	on f.shift_startdate_id = btz.date_id
	and f.shift_startinterval_id = btz.interval_id
	and p.time_zone_id = btz.time_zone_id
inner join mart.dim_date dd
	on dd.date_id = btz.local_date_id
inner join stage.stg_schedule_changed ch
	on ch.person_code = p.person_code
	and ch.schedule_date = dd.date_date
		AND --trim
		(
				(ch.schedule_date	>= p.valid_from_date_local)

			AND
				(ch.schedule_date <= p.valid_to_date_local)
		)
WHERE f.scenario_id=@scenario_id

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

-- special delete if something is left, a shift over midninght for example
INSERT INTO #stg_schedule
SELECT
	ds.scenario_id,
	stg.interval_id,
	dsd.date_id,
	dp.person_id
FROM Stage.stg_schedule stg
INNER JOIN
	mart.dim_person		dp
ON
	stg.person_code		=			dp.person_code
	AND --trim
		(
				(stg.shift_start	>= dp.valid_from_date)
			AND
				(stg.shift_start < dp.valid_to_date)
		)
INNER JOIN mart.dim_date AS dsd 
ON stg.schedule_date = dsd.date_date
INNER JOIN mart.dim_scenario ds
	ON stg.scenario_code = ds.scenario_code
WHERE ds.scenario_id=@scenario_id

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

--return rows to ETL
SET NOCOUNT OFF

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

DELETE fs
FROM mart.fact_schedule fs
INNER JOIN #stg_schedule_changed a
	ON	a.person_id = fs.person_id
	AND a.scenario_id = fs.scenario_id
	AND a.shift_startdate_id = fs.shift_startdate_id

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

DELETE fs
FROM mart.fact_schedule fs
INNER JOIN #stg_schedule tmp
	ON tmp.person_id	= fs.person_id
	AND tmp.date_id		= fs.schedule_date_id
	AND tmp.interval_id = fs.interval_id
	AND tmp.scenario_id = fs.scenario_id

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

--insert new and updated
--disable FK
ALTER TABLE mart.fact_schedule NOCHECK CONSTRAINT ALL

INSERT INTO mart.fact_schedule
	(
	schedule_date_id, 
	person_id, 
	interval_id, 
	activity_starttime, 
	scenario_id, 
	activity_id, 
	absence_id, 
	activity_startdate_id, 
	activity_enddate_id, 
	activity_endtime, 
	shift_startdate_id, 
	shift_starttime, 
	shift_enddate_id, 
	shift_endtime, 
	shift_startinterval_id, 
	shift_category_id, 
	shift_length_id, 
	scheduled_time_m, 
	scheduled_time_absence_m,
	scheduled_time_activity_m,
	scheduled_contract_time_m,
	scheduled_contract_time_activity_m,
	scheduled_contract_time_absence_m,
	scheduled_work_time_m, 
	scheduled_work_time_activity_m,
	scheduled_work_time_absence_m,
	scheduled_over_time_m, 
	scheduled_ready_time_m,
	scheduled_paid_time_m,
	scheduled_paid_time_activity_m,
	scheduled_paid_time_absence_m,
	last_publish, 
	business_unit_id,
	datasource_id, 
	datasource_update_date,
	overtime_id
	)
SELECT * FROM [Stage].[v_stg_schedule_load]

--enable FK
ALTER TABLE mart.fact_schedule CHECK CONSTRAINT ALL

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

if @debug=1
begin
	select * from @timeStat
end
GO
