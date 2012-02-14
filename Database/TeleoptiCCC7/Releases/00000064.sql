/* 
BuildTime is: 
2009-02-05 
12:07
*/ 
/* 
Trunk initiated: 
2009-01-28 
11:43
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
SET NOCOUNT ON
----------------  
--Name: David Jonsson
--Date: 2009-02-02
--Desc: Datamart user needs UTC as default time zone  
----------------  
DECLARE @applicationlogonName nvarchar(50)
SET @applicationlogonName = 'datamart'

IF (SELECT count(id) FROM person WHERE applicationlogonName =@applicationlogonName) = 1
BEGIN
	UPDATE person
	SET DefaultTimeZone = 'UTC'
	WHERE applicationlogonName = @applicationlogonName
END
GO


----------------  
--Name: Karin
--Date: 2009-02-05
--Desc: New table stg_queue for loading Queue data from File Import  
----------------  

/****** Object:  Table [dbo].[stg_queue]    Script Date: 02/02/2009 10:41:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[stg_queue](
	[date] [datetime] NOT NULL,
	[interval] [nvarchar](50) NOT NULL,
	[queue_code] [int] NULL,
	[queue_name] [nvarchar](50) NOT NULL,
	[offd_direct_call_cnt] [int] NULL,
	[overflow_in_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
	[overflow_out_call_cnt] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[queued_and_answ_call_dur] [int] NULL,
	[queued_and_aband_call_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[queued_answ_longest_que_dur] [int] NULL,
	[queued_aband_longest_que_dur] [int] NULL,
	[avg_avail_member_cnt] [int] NULL,
	[ans_servicelevel_cnt] [int] NULL,
	[wait_dur] [int] NULL,
	[aband_short_call_cnt] [int] NULL,
	[aband_within_sl_cnt] [int] NULL
) ON [PRIMARY]

GO

/****** Object:  View [dbo].[v_stg_queue]    Script Date: 02/02/2009 14:54:29 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_stg_queue]'))
DROP VIEW [dbo].[v_stg_queue]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_stg_queue]
AS
SELECT     date, interval, queue_code, queue_name, offd_direct_call_cnt, overflow_in_call_cnt, aband_call_cnt, overflow_out_call_cnt, answ_call_cnt, 
                      queued_and_answ_call_dur, queued_and_aband_call_dur, talking_call_dur, wrap_up_dur, queued_answ_longest_que_dur, 
                      queued_aband_longest_que_dur, avg_avail_member_cnt, ans_servicelevel_cnt, wait_dur, aband_short_call_cnt, aband_within_sl_cnt
FROM         dbo.stg_queue

GO


  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitStringString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[SplitStringString]
GO

CREATE   FUNCTION [dbo].[SplitStringString]
-- Takes an input string with strings separated by commas and
-- inserts the result into a field called id in a given table 
-- with name @table_name
--
-- Created: 990322 by viktor.edlund@advisorconsulting.se
-- Last changed: 990513 by viktor.edlund@advisorconsulting.se
-- Last changed: 990819 by Micke
-- Omgjord till en funktion Ola 2004-11-09
-- returnerar en tabell ist√§llet
(@string_string varchar(8000))
RETURNS @strings TABLE (string varchar(100) NOT NULL)
As
BEGIN 

 DECLARE @pos int
 DECLARE @string varchar(50)
 DECLARE @insert_text varchar(100)
 -- Exit if an empty string is given 
 IF @string_string = '' BEGIN
  RETURN 
 END 
 -- For simplicty concatenate , at the end of the string
 SELECT @string_string = @string_string + ','
 -- Ensure that @pos <> 0  
 SELECT @pos = CHARINDEX(',', @string_string )
 WHILE @pos <> 0 BEGIN
  -- Get the position of the first ,
  SELECT @pos = CHARINDEX(',', @string_string )
  
  -- Exit?
  IF @pos = 0 OR @pos = 1 OR @string_string = ','
   return
  -- Extract the substring
  SELECT @string = SUBSTRING(@string_string,1,@pos-1)
  -- Skip leading blanks
  SELECT @string = LTRIM(@string)
  -- Extract everything except the string
  SELECT @string_string = STUFF (@string_string,1,@pos,'')
  -- Insert the string into the return table
	INSERT INTO @strings
	SELECT @string
  
 END

RETURN

END


GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitStringInt]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[SplitStringInt]
GO



-- SELECT * FROM SplitStringInt('1,2,3,4,5,6,7,8,9')

CREATE   FUNCTION [dbo].[SplitStringInt]
-- Takes an input string with strings separated by commas and
-- inserts the result into a field called id in a given table 
-- with name @table_name
--
-- Created: 990322 by viktor.edlund@advisorconsulting.se
-- Last changed: 990513 by viktor.edlund@advisorconsulting.se
-- Last changed: 990819 by Micke
-- Omgjord till en funktion Ola 2004-11-09
-- returnerar en tabell ist√§llet
(@string_string varchar(8000))
RETURNS @strings TABLE (id int NOT NULL)
As
BEGIN 

 DECLARE @pos int
 DECLARE @string varchar(50)
 DECLARE @insert_text varchar(100)
 -- Exit if an empty string is given 
 IF @string_string = '' BEGIN
  RETURN 
 END 
 -- For simplicty concatenate , at the end of the string
 SELECT @string_string = @string_string + ','
 -- Ensure that @pos <> 0  
 SELECT @pos = CHARINDEX(',', @string_string )
 WHILE @pos <> 0 BEGIN
  -- Get the position of the first ,
  SELECT @pos = CHARINDEX(',', @string_string )
  
  -- Exit?
  IF @pos = 0 OR @pos = 1 OR @string_string = ','
   return
  -- Extract the substring
  SELECT @string = SUBSTRING(@string_string,1,@pos-1)
  -- Skip leading blanks
  SELECT @string = LTRIM(@string)
  -- Extract everything except the string
  SELECT @string_string = STUFF (@string_string,1,@pos,'')
  -- Insert the string into the return table
	INSERT INTO @strings
	SELECT @string
  
 END

RETURN

END




GO

  
GO  
 
/****** Object:  StoredProcedure [dbo].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_v_stg_queue_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_v_stg_queue_delete]
GO
/****** Object:  StoredProcedure [dbo].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2009-02-02>
-- Description:	<Delete data from v_stg_queue>
-- =============================================
CREATE PROCEDURE [dbo].[raptor_v_stg_queue_delete]

AS
BEGIN
	DELETE FROM v_stg_queue
END
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_statistics_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_statistics_load]
GO
-- =============================================
-- Author:		Unknown
-- Create date: 2008-xx-xx
-- Description:	Return the queue workload used in "prepare workload"
-- Change date:	2008-12-02
--				DJ: Use existing functions to split input strings
-- =============================================
CREATE PROCEDURE [dbo].[raptor_statistics_load] 
(@QueueList		varchar(1024),		
@DateFromList	varchar(1024),
@DateToList		varchar(1024)
)
AS
BEGIN
	SET NOCOUNT ON;
	--Declares
	DECLARE @TempList table
	(
	QueueID int
	)

	DECLARE	@TempDateFromList table
	(
	ID_num int IDENTITY(1,1),
	DateFrom smallDateTime
	)

	DECLARE	@TempDateToList table
	(
	ID_num int IDENTITY(1,1),
	DateTo smallDateTime
	)

	DECLARE @TempFromToDates table
	(
	ID_num int,
	DateFrom smalldatetime,
	DateTo smalldatetime
	)

	--Init
	INSERT INTO @TempList
	SELECT * FROM SplitStringInt(@QueueList)

	INSERT INTO @TempDateFromList
	SELECT * FROM SplitStringString(@DateFromList)

	INSERT INTO @TempDateToList
	SELECT * FROM SplitStringString(@DateToList)

	INSERT INTO @TempFromToDates
	SELECT fromDates.ID_num, fromDates.DateFrom, toDates.DateTo
	FROM @TempDateFromList as fromDates
	INNER JOIN @TempDateToList as toDates ON fromDates.ID_num = toDates.ID_num

	--Return result set to client
	SELECT	
		DATEADD(mi, DATEDIFF(mi,'1900-01-01',i.interval_start), d.date_date) as Interval, 
		ql.offered_calls as StatCalculatedTasks,
		ql.abandoned_calls as StatAbandonedTasks, 
		ql.abandoned_short_calls as StatAbandonedShortTasks, 
		ql.abandoned_calls_within_SL as StatAbandonedTasksWithinSL, 
		ql.answered_calls as StatAnsweredTasks,
		ql.answered_calls_within_SL as StatAnsweredTasksWithinSL,
		ql.overflow_out_calls as StatOverflowOutTasks,
		ql.overflow_in_calls as StatOverflowInTasks,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.talk_time_s, 0)
			ELSE ISNULL(ql.talk_time_s/ql.answered_calls, 0)
		END AS StatAverageTaskTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.after_call_work_s, 0)
			ELSE ISNULL(ql.after_call_work_s/ql.answered_calls, 0)
		END AS StatAverageAfterTaskTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.speed_of_answer_s, 0)
			ELSE ISNULL(ql.speed_of_answer_s/ql.answered_calls, 0)
		END AS StatAverageQueueTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.handle_time_s, 0)
			ELSE ISNULL(ql.handle_time_s/ql.answered_calls, 0)
		END AS StatAverageHandleTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.time_to_abandon_s, 0)
			ELSE ISNULL(ql.time_to_abandon_s/ql.answered_calls, 0)
		END AS StatAverageTimeToAbandonSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.longest_delay_in_queue_answered_s, 0)
			ELSE ISNULL(ql.longest_delay_in_queue_answered_s/ql.answered_calls, 0)
		END AS StatAverageTimeLongestInQueueAnsweredSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.longest_delay_in_queue_abandoned_s, 0)
			ELSE ISNULL(ql.longest_delay_in_queue_abandoned_s/ql.answered_calls, 0)
		END AS StatAverageTimeLongestInQueueAbandonedSeconds
	FROM		dbo.fact_queue ql 
	INNER JOIN	dbo.dim_date d
		ON ql.date_id = d.date_id 
	INNER JOIN	dbo.dim_interval i
		ON ql.interval_id = i.interval_id 
	INNER JOIN	dbo.dim_queue q
		ON ql.queue_id = q.queue_id 
	WHERE q.queue_code IN (SELECT QueueID FROM @TempList)
	AND EXISTS
			(SELECT * FROM @TempFromToDates 
			WHERE DATEADD(mi, DATEDIFF(mi,'1900-01-01',i.interval_start), d.date_date) BETWEEN DateFrom and DateTo)
END
GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_reports_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_reports_load]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty list fo reports. E.g fake the Analytics reports
-- Change:		
-- =============================================

CREATE PROCEDURE [dbo].[raptor_reports_load]
AS

CREATE TABLE #report(
	[report_id] [int] NOT NULL,
	[url] [nvarchar](500) NULL,
	[report_name_resource_key] [nvarchar](50) NOT NULL
)

SELECT	report_id						as ReportId,
		'xx' + report_name_resource_key as ReportName, 
                                    url as ReportUrl
FROM #report  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_load_queues]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_load_queues]
GO

CREATE PROCEDURE dbo.[raptor_load_queues] 
                            AS
                            BEGIN
	                            SET NOCOUNT ON;
                                SELECT	queue_agg_id QueueAggId, 
		                                queue_code CtiQueueId, 
		                                queue_name [Name],
                                        queue_name [Description], 
		                                datasource_id DataSourceId,
                                        log_object_name DataSource
                                FROM dbo.dim_queue WHERE queue_id > -1
                            END
GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_load_acd_logins]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_load_acd_logins]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty result set E.g fake the Analytics ACD-logins syncing
-- Change:		
-- =============================================

CREATE PROCEDURE dbo.[raptor_load_acd_logins] 
AS

--Create teporaty table
CREATE TABLE #dim_acd_login(
	[acd_login_id] [int] IDENTITY(1,1) NOT NULL,
	[acd_login_agg_id] [int] NULL,
	[acd_login_code] [nvarchar](50) NULL,
	[acd_login_name] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[is_active] [bit] NULL,
	[datasource_id] [smallint] NULL
)

--Select empty result set for Freemimum
SELECT	acd_login_id LogOnId,
		acd_login_agg_id LogOnAggId, 
		acd_login_code [LogOnCode], 
		acd_login_name [LogOnName],
		is_active [Active],
		datasource_id DataSourceId 
FROM #dim_acd_login
GO

  
GO  
 
/****** Object:  StoredProcedure [dbo].[raptor_fact_queue_load]    Script Date: 02/02/2009 14:00:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_fact_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_fact_queue_load]
GO
/****** Object:  StoredProcedure [dbo].[raptor_fact_queue_load]    Script Date: 02/02/2009 14:00:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ
-- Create date: <2009-02-02>
-- Update date: <2009-02-04>
-- Description:	<File Import - Loads data to fact_queue from stg_queue>
--				This procedure is for TeleoptiCCC database for Freeemium case, NOT same procedure as in TeleoptiAnalytics database(even though same name). Does not handle timezones.
-- =============================================
CREATE PROCEDURE [dbo].[raptor_fact_queue_load] 
AS
BEGIN
--DECLARE
DECLARE @datasource_id smallint
DECLARE @log_object_name nvarchar(100)

--INIT
SET @datasource_id = 1
SET @log_object_name  = 'Teleopti CCC - File import'

--tidszoner, hur gîra?
--DECLARE @time_zone_id smallint
--SELECT 
--	@time_zone_id = ds.time_zone_id
--FROM
--	v_sys_datasource ds
--WHERE 
--	ds.datasource_id= @datasource_id

--CREATE TABLE #bridge_time_zone(date_id int,interval_id int,time_zone_id int,local_date_id int,local_interval_id int)
--
----H«œmta datum och intervall grupperade s«æ vi slipper dubletter vid sommar-vintertid
--INSERT #bridge_time_zone(date_id,time_zone_id,local_date_id,local_interval_id)
--SELECT	min(date_id),	time_zone_id, 	local_date_id,	local_interval_id
--FROM bridge_time_zone 
--WHERE time_zone_id	= @time_zone_id	
--AND local_date_id BETWEEN @start_date_id AND @end_date_id
--GROUP BY time_zone_id, local_date_id,local_interval_id
--
--UPDATE #bridge_time_zone
--SET interval_id= bt.interval_id
--FROM 
--(SELECT date_id,local_date_id,local_interval_id,interval_id= MIN(interval_id)
--FROM bridge_time_zone
--WHERE time_zone_id=@time_zone_id
--GROUP BY date_id,local_date_id,local_interval_id)bt
--INNER JOIN #bridge_time_zone temp ON temp.local_interval_id=bt.local_interval_id
--AND temp.date_id=bt.date_id
--AND temp.local_date_id=bt.local_date_id


DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

DECLARE @max_date smalldatetime
DECLARE @min_date smalldatetime


SELECT  
	@min_date= min(date),
	@max_date= max(date)
FROM
	v_stg_queue
 

SET	@min_date = convert(smalldatetime,floor(convert(decimal(18,4),@min_date )))
SET @max_date	= convert(smalldatetime,floor(convert(decimal(18,4),@max_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @min_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @max_date = date_date)

--ANALYZE AND UPDATE DATA IN TEMPORARY TABLE
SELECT *
INTO #stg_queue
FROM v_stg_queue

UPDATE #stg_queue
SET queue_code = d.queue_code
FROM dim_queue d
INNER JOIN #stg_queue stg ON stg.queue_name=d.queue_name
AND d.datasource_id = @datasource_id  
WHERE (stg.queue_code is null OR stg.queue_code='')

ALTER TABLE  #stg_queue ADD interval_id smallint

UPDATE #stg_queue
SET interval_id= i.interval_id
FROM dim_interval i
INNER JOIN #stg_queue stg ON stg.interval=LEFT(i.interval_name,5)

-- Delete rows for the queues imported from file
DELETE FROM fact_queue  
WHERE local_date_id BETWEEN @start_date_id 
AND @end_date_id AND datasource_id = @datasource_id
AND queue_id IN (SELECT queue_id from dim_queue dq INNER JOIN #stg_queue stg ON dq.queue_code=stg.queue_code WHERE dq.datasource_id = @datasource_id )


INSERT INTO dbo.fact_queue
	(
	date_id, 
	interval_id, 
	queue_id, 
	local_date_id,
	local_interval_id, 
	offered_calls, 
	answered_calls, 
	answered_calls_within_SL, 
	abandoned_calls, 
	abandoned_calls_within_SL, 
	abandoned_short_calls, 
	overflow_out_calls,
	overflow_in_calls,
	talk_time_s, 
	after_call_work_s, 
	handle_time_s, 
	speed_of_answer_s, 
	time_to_abandon_s, 
	longest_delay_in_queue_answered_s,
	longest_delay_in_queue_abandoned_s,
	datasource_id, 
	insert_date, 
	update_date, 
	datasource_update_date
	)
SELECT
	date_id						= d.date_id,--bridge.date_id, 
	interval_id					= stg.interval_id,--bridge.interval_id, 
	queue_id					= q.queue_id, 
	local_date_id				= d.date_id,
	local_interval_id			= stg.interval_id, 
	offered_calls				= ISNULL(offd_direct_call_cnt,0), 
	answered_calls				= ISNULL(answ_call_cnt,0), 
	answered_calls_within_SL	= ISNULL(ans_servicelevel_cnt,0), 
	abandoned_calls				= ISNULL(aband_call_cnt,0), 
	abandoned_calls_within_SL	= ISNULL(aband_within_sl_cnt,0), 
	abandoned_short_calls		= ISNULL(aband_short_call_cnt,0), 
	overflow_out_calls			= ISNULL(overflow_out_call_cnt,0),
	overflow_in_calls			= ISNULL(overflow_in_call_cnt,0), 
	talk_time_s					= ISNULL(talking_call_dur,0), 
	after_call_work_s			= ISNULL(wrap_up_dur,0), 
	handle_time_s				= ISNULL(talking_call_dur,0)+ISNULL(wrap_up_dur,0), 
	speed_of_answer_s			= ISNULL(queued_and_answ_call_dur,0), 
	time_to_abandon_s			= ISNULL(queued_and_aband_call_dur,0), 
	longest_delay_in_queue_answered_s = ISNULL(queued_answ_longest_que_dur,0),
	longest_delay_in_queue_abandoned_s = ISNULL(queued_aband_longest_que_dur,0),
	datasource_id				= q.datasource_id, 
	insert_date					= getdate(), 
	update_date					= getdate(), 
	datasource_update_date		= '1900-01-01'

FROM
	(SELECT * FROM #stg_queue WHERE date between @min_date and @max_date) stg
JOIN
	dim_date		d
ON
	stg.date	= d.date_date
JOIN 
	dim_interval i
ON 
	i.interval_id=stg.interval_id
JOIN
	dim_queue		q
ON
	q.queue_code= stg.queue_code 
	AND q.datasource_id = @datasource_id

END  
GO  
 
/****** Object:  StoredProcedure [dbo].[raptor_dim_queue_load]    Script Date: 02/02/2009 14:00:15 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_dim_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_dim_queue_load]
GO
/****** Object:  StoredProcedure [dbo].[raptor_dim_queue_load]    Script Date: 02/02/2009 14:00:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2009-02-02>
-- Update date :<2009-02-02>
-- Description:	<<File Import - Loads data to dim_queue from stg_queue>
-- =============================================
CREATE PROCEDURE [dbo].[raptor_dim_queue_load] 
	
AS
BEGIN
--DECLARE
DECLARE @datasource_id smallint
DECLARE @log_object_name nvarchar(100)

--INIT
SET @datasource_id = 1
SET @log_object_name  = 'Teleopti CCC - File import'

--------------------------------------------------------------
-- Insert Not Defined queue
SET IDENTITY_INSERT dim_queue ON
INSERT INTO dbo.dim_queue
	(
	queue_id,
	queue_name,
	datasource_id	
	)
SELECT 
	queue_id			=-1,
	queue_name			='Not Defined',
	datasource_id		=-1
WHERE NOT EXISTS (SELECT * FROM dim_queue where queue_id = -1)
SET IDENTITY_INSERT dim_queue OFF

--Update
--Existing queues with a queue_code in the importfile
UPDATE dim_queue
SET
	queue_code		=stg.queue_code, 
	queue_name		=stg.queue_name,
	log_object_name =@log_object_name, 
	update_date		= getdate()
FROM
	v_stg_queue stg	
JOIN
	dim_queue
ON
	dim_queue.queue_code		= stg.queue_code 			AND
	dim_queue.datasource_id		= @datasource_id
WHERE stg.queue_code IS NOT NULL

--------------------------------------------------------------
--Update
--Existing queues without a queue_code (fallback on queue_name)
UPDATE dim_queue
SET 
	queue_code		=q.queue_id, 
	queue_name		=stg.queue_name,
	log_object_name =@log_object_name, 
	update_date		= getdate()
FROM
	v_stg_queue stg	
JOIN
	dim_queue q
ON
	q.queue_name		= stg.queue_name 			AND
	q.datasource_id		= @datasource_id
WHERE stg.queue_code IS NULL

---------------------------------------------------------------
-- Reset identity seed.
DECLARE @max_id INT
SET @max_id= (SELECT max(queue_id) FROM dim_queue)

DBCC CHECKIDENT ('dim_queue',reseed,@max_id);
---------------------------------------------------------------------------
-- Insert new queues with a queue_code
INSERT INTO dbo.dim_queue
	( 
	queue_agg_id,
	queue_code, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_code				= stg.queue_code, 
	queue_name				= max(stg.queue_name),
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	v_stg_queue stg
WHERE 
	NOT EXISTS (SELECT queue_id FROM dim_queue d 
					WHERE	d.queue_code= stg.queue_code 	AND
							d.datasource_id =@datasource_id
				)
AND stg.queue_code IS NOT NULL
GROUP BY stg.queue_code

----------------------------------------------------------------------------------------
-- Insert new queues without a queue_code (fallback on queue_name)
INSERT INTO dbo.dim_queue
	( 
	queue_agg_id,
	queue_code, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_code				= NULL, 
	queue_name				= stg.queue_name,
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	v_stg_queue stg
WHERE 
	NOT EXISTS (SELECT queue_id FROM dim_queue d 
					WHERE	d.queue_name= stg.queue_name 	AND
							d.datasource_id =@datasource_id
				)
AND stg.queue_code IS NULL
GROUP BY stg.queue_name

--SET queue_agg_id AND queue_code TO SAME VALUES AS queue_id IF NO queue_code OR queue_agg_id
UPDATE dim_queue
SET queue_agg_id=queue_id, queue_code= queue_id
WHERE queue_agg_id IS NULL 
AND queue_code IS NULL
AND datasource_id=@datasource_id

UPDATE dim_queue
SET queue_agg_id=queue_code
WHERE queue_agg_id IS NULL 
AND queue_code IS NOT NULL
AND datasource_id=@datasource_id


END
GO
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_dim_queue_create]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_dim_queue_create]
GO

CREATE PROCEDURE [dbo].[raptor_dim_queue_create] 
@queue_code nvarchar(50),
@queue_name nvarchar(50)

	
AS

insert into dim_queue(queue_code, queue_name, log_object_name, queue_agg_id, datasource_id) values(@queue_code,  @queue_name, 'importfile', 20, 1)
select @@identity   
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[raptor_dim_date_getId_ByDate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[raptor_dim_date_getId_ByDate]
GO

--[raptor_dim_date_getId_ByDate] '20050101,19991231,19981223,19881212'
-- =============================================
-- Author:		DJ
-- Create date: 2009-01-26
-- Description:	Get the date_ids from a string of dates
--				Format must be [yyyymmdd]
--				Used when importing Workload data in Forecasts module
-- Change date:	2009-xx-xx
-- By:			
--				
--				
--				
-- =============================================

CREATE PROCEDURE [dbo].[raptor_dim_date_getId_ByDate] 
@DateList varchar(8000)
AS

DECLARE	@TempDateList table
(
Date_date smallDateTime
)


--Get string into temp table
INSERT INTO @TempDateList
SELECT * FROM SplitStringString(@DateList)

--Return result set
SELECT	ISNULL(d.date_id,-1) as date_id,
		temp.date_date as date_date
FROM @TempDateList temp
LEFT OUTER JOIN dim_date d
ON temp.Date_date = d.date_date
GO
  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (64,'7.0.64') 
