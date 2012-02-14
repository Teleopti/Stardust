/* 
Trunk initiated: 
2010-06-10 
13:58
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: DJ
--Date: 2010-06-10
--Desc: Bug #10592, Adding info about logged in true/false to fact_schedule_adherance
----------------
PRINT 'Re-design mart.fact_schedule_deviation... '

ALTER TABLE mart.fact_schedule_deviation ADD is_logged_in bit NULL
GO
--0 is the current behaviour
UPDATE mart.fact_schedule_deviation
SET is_logged_in = 0
GO
ALTER TABLE mart.fact_schedule_deviation ALTER COLUMN is_logged_in bit NOT NULL
PRINT 'Re-design mart.fact_schedule_deviation. Done'

--Load new Fn (added later in the deploy sequence)
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[DimPersonAdapted]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[DimPersonAdapted]
GO


-- =============================================
-- Author:		Jonas
-- Create date: 2010-03-24
-- Description:	Returns a table with Person information where valid_to_date and 
-- valid_to_interval_id are adapted to eternuty date 2059-12-31. This to make dim_person more "joinable".
-- =============================================
CREATE FUNCTION [mart].[DimPersonAdapted] 
(
)
RETURNS 
@dim_person TABLE 
(
	person_id int, 
	person_name nvarchar(200),
	valid_from_date smalldatetime, 
	valid_to_date smalldatetime, 
	valid_from_date_id int, 
	valid_to_date_id int, 
	valid_from_interval_id int, 
	valid_to_interval_id int,
	team_id int, 
	team_name nvarchar(100)
)
AS
BEGIN

DECLARE @interval_length int, @intervals_per_day int, @max_date smalldatetime
SELECT @interval_length = DATEDIFF(mi, interval_start, interval_end) FROM mart.dim_interval WHERE interval_id = 0
SET @intervals_per_day = 1440/@interval_length
SELECT @max_date = MAX(date_date) FROM mart.dim_date WHERE date_date <> '20591231'

INSERT INTO @dim_person (person_id, person_name, valid_from_date, valid_to_date, team_id, team_name)
SELECT	person_id, 
		person_name,
		valid_from_date,
		CASE WHEN valid_to_date = '20591231'
			THEN @max_date
			ELSE	CASE WHEN (DATEDIFF(mi, convert(smalldatetime, convert(int, (convert(float, valid_to_date)))), valid_to_date)) = 0
						THEN DATEADD(d, -1, valid_to_date)
						ELSE valid_to_date
					END
		END AS 'valid_to_date',
		team_id,
		team_name
FROM mart.dim_person

UPDATE @dim_person
SET valid_from_date_id = d1.date_id,
	valid_to_date_id = d2.date_id,
	valid_from_interval_id = ((datepart(hh, p.valid_from_date)*60) + datepart(mi, p.valid_from_date)) / @interval_length, 
valid_to_interval_id = CASE WHEN (((datepart(hh, p.valid_to_date)*60) + datepart(mi, p.valid_to_date)) / @interval_length) = 0
						THEN @intervals_per_day -1
						ELSE (((datepart(hh, p.valid_to_date)*60) + datepart(mi, p.valid_to_date)) / @interval_length) -1
					END
FROM @dim_person p
INNER JOIN mart.dim_date d1
	ON d1.date_date = convert(smalldatetime, convert(int, (convert(float, p.valid_from_date))))
INNER JOIN mart.dim_date d2
	ON d2.date_date = convert(smalldatetime, convert(int, (convert(float, p.valid_to_date))))
	
RETURN 

END

GO

--Load new Sp (added later in the deploy sequence)
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
	scheduled_ready_time_m int,
	ready_time_m int,
	is_logged_in int, 
	contract_time_m int,
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
	ready_time_m,
	is_logged_in,
	business_unit_id
	)
SELECT
	date_id					= fa.date_id, 
	interval_id				= fa.interval_id,
	person_id				= b.person_id, 
	ready_time_m			= fa.ready_time_s,
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
	scheduled_ready_time_m,
	contract_time_m,
	business_unit_id
	)
SELECT
	date_id					= fs.schedule_date_id, 
	interval_id				= fs.interval_id,
	person_id				= fs.person_id,
	is_logged_in			= 0, --Mark schedule rows as Not loggged in 
	scheduled_ready_time_m	= fs.scheduled_ready_time_m,
	contract_time_m			= fs.scheduled_contract_time_m,
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
	scheduled_ready_time_m,
	ready_time_m,
	is_logged_in,
	contract_time_m,
	business_unit_id,
	datasource_id, 
	insert_date, 
	update_date
	)
SELECT
	date_id					= date_id, 
	interval_id				= interval_id,
	person_id				= person_id, 
	scheduled_ready_time_m	= sum(isnull(scheduled_ready_time_m,0)),
	ready_time_m			= convert(int,ROUND(sum(isnull(ready_time_m,0))/60.0,0)),
	is_logged_in			= sum(is_logged_in), --Calculated bit value
	contract_time_m			= sum(isnull(contract_time_m,0)),
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
SET deviation_schedule_m = scheduled_ready_time_m-ready_time_m
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_m>=ready_time_m

-- b) Over Performance
UPDATE mart.fact_schedule_deviation
SET deviation_schedule_m =
	CASE
		WHEN scheduled_ready_time_m = 0 THEN ready_time_m
		ELSE CAST(ready_time_m-scheduled_ready_time_m AS decimal(18,4)) * CAST(scheduled_ready_time_m AS decimal(18,4))/CAST(ready_time_m AS decimal(18,4))
	END
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_m<ready_time_m

--Special case for both [deviation_schedule_ready_m] and [deviation_contract_m]
UPDATE	mart.fact_schedule_deviation
SET
	deviation_schedule_ready_m = 0,
	deviation_contract_m = 0
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_m=0

--2 Deviation_schedule_ready, only intervals where agents are scheduled to be ready are included
-- a) Under performance
UPDATE	mart.fact_schedule_deviation
SET	deviation_schedule_ready_m = scheduled_ready_time_m-ready_time_m
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_m>0
AND scheduled_ready_time_m>=ready_time_m

-- b) Over performance
UPDATE	mart.fact_schedule_deviation
--Bug #11813 SET	deviation_schedule_ready_m = CAST(ready_time_m-scheduled_ready_time_m AS decimal(18,4)) * CAST(scheduled_ready_time_m AS decimal(18,4))/CAST(ready_time_m AS decimal(18,4)) 
SET	deviation_schedule_ready_m = 0 --corrected
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_m>0
AND scheduled_ready_time_m<ready_time_m

--3 Deviation_contract, only intervals where agents are contracted to be working are included
-- a) Under performance
UPDATE mart.fact_schedule_deviation
SET	deviation_contract_m = contract_time_m-ready_time_m
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_m>0
AND contract_time_m>=ready_time_m

-- b) Over performance
UPDATE mart.fact_schedule_deviation
SET	deviation_contract_m = CAST(ready_time_m-contract_time_m AS decimal(18,4)) * CAST(contract_time_m AS decimal(18,4))/CAST(ready_time_m AS decimal(18,4))
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_m>0
AND contract_time_m<ready_time_m

GO
--
--Print re-load deviation command
SET NOCOUNT ON
PRINT ''
PRINT 'Adherance figures will be update by DBManager. Please wait ...'

PRINT '-----'
DECLARE @cmd nvarchar(4000)
DECLARE @BuCode uniqueidentifier
DECLARE @BuName nvarchar(100)

DECLARE BU_cursor CURSOR FOR
	SELECT business_unit_code, business_unit_name FROM mart.dim_business_unit where business_unit_id <> -1

OPEN BU_cursor 

FETCH NEXT FROM BU_cursor INTO @BuCode,@BuName

WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT 'Updating Adherance figures for BU: ' + @BuName + ' ...'
	SELECT @cmd='EXEC ' +db_name()+'.mart.etl_fact_schedule_deviation_load ''2000-01-01 00:00:00'','''+CONVERT(VARCHAR(19), GETDATE(), 120)+''','''+ cast(@BuCode as nvarchar(100)) +''''
	EXEC sp_executesql @cmd
	FETCH NEXT FROM BU_cursor INTO @BuCode,@BuName
END

CLOSE BU_cursor 
DEALLOCATE BU_cursor 
PRINT 'Done!'
PRINT '-----'
SET NOCOUNT OFF 
GO 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (282,'7.1.282') 
