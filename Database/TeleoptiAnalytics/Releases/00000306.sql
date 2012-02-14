/* 
Trunk initiated: 
2010-10-26 
08:02
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2010-xx-xx  
--Desc: Because ...  
----------------  


----------------  
--Name: Karin Jeppsson  
--Date: 2010-11-01  
--Desc: #11055 All deviation calculations now in seconds  
----------------  
EXEC sp_rename 'mart.fact_schedule_deviation.scheduled_ready_time_m', 'scheduled_ready_time_s', 'COLUMN';
EXEC sp_rename 'mart.fact_schedule_deviation.ready_time_m', 'ready_time_s', 'COLUMN';
EXEC sp_rename 'mart.fact_schedule_deviation.contract_time_m', 'contract_time_s', 'COLUMN';
EXEC sp_rename 'mart.fact_schedule_deviation.deviation_schedule_m', 'deviation_schedule_s', 'COLUMN';
EXEC sp_rename 'mart.fact_schedule_deviation.deviation_schedule_ready_m', 'deviation_schedule_ready_s', 'COLUMN';
EXEC sp_rename 'mart.fact_schedule_deviation.deviation_contract_m', 'deviation_contract_s', 'COLUMN';
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_deviation_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_deviation_load]
GO
-- =============================================
-- Author:		ChLu
-- Description:	Write schedule deviation from fact_agent,fact_schedule and fact_contract tables 
--				to data mart table 'fact_schedule_viation'.
-- Updates:		2008-09-02 Added parameters @start_date and @end_date
--				2008-12-03 Removed refernce to fact_contract, contract_time now from fact_schedule KJ
--				2009-02-11 New mart schema KJ
--				2009-07-07 Added new #-table for summarizing KJ
--				2009-09-16 Changed the way we treat deviation_m for over and under performance DJ
--				2009-10-01 Include rows for agents with Schedule but without agent statistics
--				2009-10-01 Removed isnull in calculation
--				2009-10-01 Special CASE WHEN scheduled_ready_time_m = 0 THEN SET deviation_schedule_m = ready_time_m
--				2010-06-04 Need to consider scenario when doing calculation
--				2010-06-10 Added detection of is_logged_in, used when ready_time_m = 0 to show zero, else empty string in report
--				2010-09-20 Fix calculation: Ready Time vs. Scheduled Ready Time. Set 100% as soon a mixed interval is fullfilled. e.g Readytime >= ScheduleReadytime
--				2010-11-01 #11055 Refact of mart.fact_schedule_deviation, measures in seconds instead of minutes. KJ
-- =============================================

--exec mart.etl_fact_schedule_deviation_load '2009-02-01 23:00:00','2009-02-03 23:00:00','928DD0BC-BF40-412E-B970-9B5E015AADEA' --Demo
CREATE PROCEDURE [mart].[etl_fact_schedule_deviation_load]
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
AS

DECLARE @start_date_id int
DECLARE @end_date_id int
DECLARE @max_date_id int
DECLARE @min_date_id int
DECLARE @business_unit_id int
DECLARE @scenario_id int


CREATE TABLE #fact_schedule_deviation(
	date_id int,
	interval_id smallint,
	person_id int,
	scheduled_ready_time_s int,--20101029
	ready_time_s int,--20101029
	is_logged_in int, 
	contract_time_s int,--20101029
	business_unit_id int
)

/*Remove timestamp from datetime*/
SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,8),@start_date )))
SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,8),@end_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

SET @scenario_id	=	(SELECT scenario_id FROM mart.dim_scenario WHERE business_unit_code = @business_unit_code AND default_scenario = 1)

/*Get max and min date from fact_agent(can not calculate deviation unless data here)*/
--SELECT
--	@max_date_id= max(date_id),
--	@min_date_id= min(date_id)
--FROM
--	mart.fact_agent

/*Change date interval accordning to min and max date in fact_agent*/
--SET	@start_date_id = CASE WHEN @min_date_id > @start_date_id THEN @min_date_id ELSE @start_date_id END
--SET	@end_date_id	= CASE WHEN @max_date_id < @end_date_id	THEN @max_date_id ELSE @end_date_id	END

/*Get business unit id*/
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

/*Remove old data*/
DELETE FROM mart.fact_schedule_deviation
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id


--20090707 KJ
/*MAKE SURE THERE IS ONLY ONE ROW PER INTERVAL(IF SEVERAL ACTIVITIES PER INTERVAL)*/
SELECT
	 fs.schedule_date_id, 
	 fs.interval_id,
	 fs.person_id, 
	sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
	sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
	fs.business_unit_id
INTO 	#fact_schedule
FROM 
	mart.fact_schedule fs
WHERE fs.schedule_date_id BETWEEN @start_date_id AND @end_date_id
	AND fs.business_unit_id = @business_unit_id
	AND fs.scenario_id = @scenario_id
GROUP BY 
	fs.schedule_date_id, 
	fs.interval_id,
	fs.person_id,
	fs.business_unit_id



/* Prepare insert of new data in two steps, a) and b). */
/* a) Gather agent ready time */
INSERT INTO #fact_schedule_deviation
	(
	date_id, 
	interval_id,
	person_id, 
	ready_time_s,--20101029
	is_logged_in,
	business_unit_id
	)
SELECT
	date_id					= fa.date_id, 
	interval_id				= fa.interval_id,
	person_id				= b.person_id, 
	ready_time_s			= fa.ready_time_s,--20101029
	is_logged_in			= 1, --marks that we do have logged in time
	business_unit_id		= b.business_unit_id
FROM 
	mart.bridge_acd_login_person b
JOIN
	mart.fact_agent fa
ON
	b.acd_login_id = fa.acd_login_id
INNER JOIN 
	mart.DimPersonAdapted() p
ON 
	p.person_id = b.person_id
	AND
		(
			(fa.date_id > p.valid_from_date_id AND fa.date_id < p.valid_to_date_id)
				OR (fa.date_id = p.valid_from_date_id AND fa.interval_id >= p.valid_from_interval_id)
				OR (fa.date_id = p.valid_to_date_id AND fa.interval_id <= p.valid_to_interval_id)
		)
WHERE
	fa.date_id BETWEEN @start_date_id AND @end_date_id
	AND b.business_unit_id = @business_unit_id

/* b) Gather agent schedule time */
INSERT INTO #fact_schedule_deviation
	(
	date_id, 
	interval_id,
	person_id,
	is_logged_in, 
	scheduled_ready_time_s,--20101029
	contract_time_s,--20101029
	business_unit_id
	)
SELECT
	date_id					= fs.schedule_date_id, 
	interval_id				= fs.interval_id,
	person_id				= fs.person_id,
	is_logged_in			= 0, --Mark schedule rows as Not loggged in 
	scheduled_ready_time_s	= fs.scheduled_ready_time_m*60,--20101029
	contract_time_s			= fs.scheduled_contract_time_m*60,--20101029
	business_unit_id		= fs.business_unit_id
FROM 
	#fact_schedule fs
INNER JOIN 
	mart.DimPersonAdapted() p
ON
	p.person_id = fs.person_id
	AND
		(
			(fs.schedule_date_id > p.valid_from_date_id AND fs.schedule_date_id < p.valid_to_date_id)
				OR (fs.schedule_date_id = p.valid_from_date_id AND fs.interval_id >= p.valid_from_interval_id)
				OR (fs.schedule_date_id = p.valid_to_date_id AND fs.interval_id <= p.valid_to_interval_id)
		)
WHERE
	fs.schedule_date_id BETWEEN @start_date_id AND @end_date_id
	AND fs.business_unit_id = @business_unit_id


/* Insert of new data */
INSERT INTO mart.fact_schedule_deviation
	(
	date_id, 
	interval_id,
	person_id, 
	scheduled_ready_time_s,--20101029
	ready_time_s,--20101029
	is_logged_in,
	contract_time_s,--20101029
	business_unit_id,
	datasource_id, 
	insert_date, 
	update_date
	)
SELECT
	date_id					= date_id, 
	interval_id				= interval_id,
	person_id				= person_id, 
	scheduled_ready_time_s	= sum(isnull(scheduled_ready_time_s,0)),--20101029
	ready_time_s			= sum(isnull(ready_time_s,0)),--20101029
	is_logged_in			= sum(is_logged_in), --Calculated bit value
	contract_time_s			= sum(isnull(contract_time_s,0)),--20101029
	business_unit_id		= business_unit_id,
	datasource_id			= -1, 
	insert_date				= getdate(), 
	update_date				= getdate()
FROM 
	#fact_schedule_deviation
GROUP BY 
	date_id, 
	interval_id,
	person_id,
	business_unit_id

--We count the three deviation_m in two ways a) or b)

--a) If Ready Time is less or equal to ScheduleReadyTime (agent under performed)
--In this case the Deviation will be the actual Diff between ScheduleReadyTime vs. ActualReadyTime
--As: Deviation_m = scheduled_ready_time_m-ready_time_m

--b) If ReadyTime is more then ScheduleReadyTime (agent over performed)
--In this case the we will be set the deviation_m in a way that in will give the correct Adherance procentage.
--As: Calculate Deviation_m = (ready_time_m-scheduled_ready_time_m)*(scheduled_ready_time_m/ready_time_m)



--See: http://teamserver/sites/TheMatrix/Standard%20Reports/Adherance.xlsx
--1 Deviation from schedule, i.e all scheduled time is included.
-- a) Under performance
UPDATE mart.fact_schedule_deviation
SET deviation_schedule_s = scheduled_ready_time_s-ready_time_s--20101029
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s>=ready_time_s--20101029

-- b) Over Performance
UPDATE mart.fact_schedule_deviation
SET deviation_schedule_s =
	CASE
		WHEN scheduled_ready_time_s = 0 THEN ready_time_s--20101029
		ELSE CAST(ready_time_s-scheduled_ready_time_s AS decimal(18,4)) * CAST(scheduled_ready_time_s AS decimal(18,4))/CAST(ready_time_s AS decimal(18,4))--20101029
	END
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s<ready_time_s--20101029

--Special case for both [deviation_schedule_ready_m] and [deviation_contract_m]
UPDATE	mart.fact_schedule_deviation
SET
	deviation_schedule_ready_s = 0,--20101029
	deviation_contract_s = 0--20101029
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s=0--20101029

--2 Deviation_schedule_ready, only intervals where agents are scheduled to be ready are included
-- a) Under performance
UPDATE	mart.fact_schedule_deviation
SET	deviation_schedule_ready_s = scheduled_ready_time_s-ready_time_s--20101029
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s>0--20101029
AND scheduled_ready_time_s>=ready_time_s--20101029

-- b) Over performance
UPDATE	mart.fact_schedule_deviation
--Bug #11813 SET	deviation_schedule_ready_m = CAST(ready_time_m-scheduled_ready_time_m AS decimal(18,4)) * CAST(scheduled_ready_time_m AS decimal(18,4))/CAST(ready_time_m AS decimal(18,4)) 
SET	deviation_schedule_ready_s = 0 --corrected  --20101029
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s>0--20101029
AND scheduled_ready_time_s<ready_time_s--20101029

--3 Deviation_contract, only intervals where agents are contracted to be working are included
-- a) Under performance
UPDATE mart.fact_schedule_deviation
SET	deviation_contract_s = contract_time_s-ready_time_s--20101029
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s>0--20101029
AND contract_time_s>=ready_time_s--20101029

-- b) Over performance
UPDATE mart.fact_schedule_deviation
SET	deviation_contract_s = CAST(ready_time_s-contract_time_s AS decimal(18,4)) * CAST(contract_time_s AS decimal(18,4))/CAST(ready_time_s AS decimal(18,4))--20101029
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s>0--20101029
AND contract_time_s<ready_time_s--20101029


GO

PRINT 'Adherance data will now we re-loaded. This will take some time (time out = 15 min)'
PRINT 'Please do not close this Windows!'

--reload data
DECLARE @min_date smalldatetime
DECLARE @max_date smalldatetime
SET @min_date = (SELECT date_date from mart.dim_date where date_id in (Select MIN(date_id) from mart.fact_schedule_deviation))
SET @max_date = (SELECT date_date from mart.dim_date where date_id in (Select MAX(date_id) from mart.fact_schedule_deviation))

DECLARE @id uniqueidentifier
DECLARE @business_unit_code uniqueidentifier 

DECLARE business_unit_Cursor CURSOR FOR
	SELECT business_unit_code
	FROM mart.dim_business_unit
	WHERE  business_unit_id<>-1
	ORDER BY business_unit_id
OPEN business_unit_Cursor;
FETCH NEXT FROM business_unit_Cursor
INTO @business_unit_code
WHILE @@FETCH_STATUS = 0
   BEGIN 
		exec mart.etl_fact_schedule_deviation_load @min_date,@max_date,@business_unit_code
		FETCH NEXT FROM business_unit_Cursor
		INTO @business_unit_code
   END;
CLOSE business_unit_Cursor;
DEALLOCATE business_unit_Cursor;

GO


----------------  
--Name: Robin Karlsson
--Date: 2010-11-05
--Desc: Adding separate stage table for skills. Must be IF EXISTS, because it was shipped with a later build of release 297.
---------------- 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_skill]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [stage].[stg_skill](
			[skill_code] [uniqueidentifier] NOT NULL,
			[skill_name] [nvarchar](100) NOT NULL,
			[time_zone_code] [nvarchar](50) NOT NULL,
			[forecast_method_code] [uniqueidentifier] NULL,
			[forecast_method_name] [nvarchar](100) NOT NULL,
			[business_unit_code] [uniqueidentifier] NOT NULL,
			[business_unit_name] [nvarchar](50) NOT NULL,
			[datasource_id] [smallint] NOT NULL,
			[insert_date] [smalldatetime] NOT NULL,
			[update_date] [smalldatetime] NOT NULL,
			[datasource_update_date] [datetime] NOT NULL,
			[is_deleted] [bit] NOT NULL,
		 CONSTRAINT [PK_stg_skill] PRIMARY KEY CLUSTERED 
		(
			[skill_code] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
		) ON [STAGE]
	END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[DF_stg_skill_is_deleted]'))
	BEGIN
		ALTER TABLE [stage].[stg_skill] ADD  CONSTRAINT [DF_stg_skill_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
	END
GO

	 
----------------  
--Name: Ola Håkansson, moved to Release 306 by David
--Date: 2010-11-10  
--Desc: Loading more than Default Scenario to  stage.stg_schedule_preference
----------------  
ALTER TABLE stage.stg_schedule_preference
	DROP CONSTRAINT PK_stg_schedule_preference
GO
ALTER TABLE stage.stg_schedule_preference ADD CONSTRAINT
	PK_stg_schedule_preference PRIMARY KEY CLUSTERED 
	(
	person_restriction_code,
	scenario_code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON STAGE
GO
  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
 ----------------  
--Name: Anders F
--Date: 2010-11-19  
--Desc: Switch to heartbeats every minute, and restart every 3 min (was 10s and 1min)  
----------------  
update msg.Configuration
set ConfigurationValue = 180000
where ConfigurationId = 10
update msg.Configuration
set ConfigurationValue = 60000
where ConfigurationId = 4

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (306,'7.1.306') 
