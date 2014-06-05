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
--  2014-02-11  KJ		#26422 - redesign for stability, load schedule on agent local date
-- =============================================
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

--if no @scenario, no data then break
DECLARE @scenario_code uniqueidentifier
DECLARE @scenario_id smallint
SELECT TOP 1 @scenario_code=scenario_code FROM Stage.stg_schedule_changed
IF @scenario_code IS NULL
BEGIN
	RETURN 0
END

--Get first row scenario in stage table, currently this must(!) be the default scenario, else RAISERROR
if (select count(*)
	from mart.dim_scenario
	where business_unit_code = @business_unit_code
	and scenario_code = @scenario_code
	and default_scenario = 1
	) <> 1
BEGIN
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = 'This is not a default scenario, or muliple default scenarios exists!'
	RAISERROR (@ErrorMsg,16,1)
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

CREATE TABLE #stg_schedule_changed(
	[person_id] [int] NOT NULL,
	[shift_startdate_local_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL
)

if @debug=1
begin
	insert into @timeStat(step,totalTime,laststep_ms) select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate();set @step=@step+1
end

--prepare a temp table for better performance on delete
INSERT INTO #stg_schedule_changed
SELECT  p.person_id,
		dd.date_id,
		s.scenario_id
FROM stage.stg_schedule_changed ch
INNER JOIN mart.dim_person p
	ON p.person_code = ch.person_code
		AND --trim
		(
				(ch.schedule_date_local	>= p.valid_from_date_local)

			AND
				(ch.schedule_date_local <= p.valid_to_date_local)
		)
INNER JOIN mart.dim_date dd
	ON dd.date_date = ch.schedule_date_local
INNER JOIN mart.dim_scenario s
	ON ch.scenario_code=s.scenario_code
WHERE s.scenario_id=@scenario_id

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
INNER JOIN #stg_schedule_changed ch
	ON	ch.person_id = fs.person_id
	AND ch.scenario_id = fs.scenario_id
	AND ch.shift_startdate_local_id = fs.shift_startdate_local_id

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
	shift_startdate_local_id,
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
	shift_endinterval_id, 
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
