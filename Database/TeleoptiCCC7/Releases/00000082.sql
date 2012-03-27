/* 
BuildTime is: 
2009-03-26 
15:28
*/ 
/* 
Trunk initiated: 
2009-03-12 
17:52
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Ola HÂkansson  
--Date: 2009-03-09  
--Desc: Changes the write protection to be default null on Team (instead of 28)  
----------------  
alter TABLE Team
alter column WriteProtection int null

GO
ALTER TABLE Team
	DROP CONSTRAINT DF_Team_DaysLockedAfter
GO

UPDATE Team SET WriteProtection = null
WHERE WriteProtection = 28
GO
----------------  
--Name: Robin Karlsson  
--Date: 2009-03-17  
--Desc: Adds domain for payroll export result  
---------------- 
CREATE TABLE [dbo].[PayrollResult](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[PayrollFormatName] [nvarchar](50) NOT NULL,
	[PayrollFormatId] [uniqueidentifier] NOT NULL,
	[Owner] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[Result] [xml] NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_BusinessUnit]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_Person_Owner] FOREIGN KEY([Owner])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_Person_Owner]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_Person_UpdatedBy]
GO

--------------
--Name: tamasb
--Date: 2009-03-18 11:22 
--Desc: Remove BusinesUnit column from AvailableData table.
--Expl: Do not need it anymore as it has a one-to-one relationship between AvailableData and ApplicationRole so the ApplicationRole fk
--		will define the AvailableData
---------------- 
ALTER TABLE dbo.AvailableData
           DROP CONSTRAINT FK_AvailableData_BusinessUnit
GO
ALTER TABLE dbo.AvailableData
           DROP COLUMN BusinessUnit
GO




----------------  
--Name: Henry Greijer
--Date: 2009-03-19
--Desc: Adds Name column, Id GUID is set as default Name.
---------------- 

ALTER TABLE dbo.PayrollExport
	DROP CONSTRAINT FK_PayrollExport_BusinessUnit
GO
GO
ALTER TABLE dbo.PayrollExport
	DROP CONSTRAINT FK_PayrollExport_Person_CreatedBy
GO
ALTER TABLE dbo.PayrollExport
	DROP CONSTRAINT FK_PayrollExport_Person_UpdatedBy
GO
GO
CREATE TABLE dbo.Tmp_PayrollExport
	(
	Id uniqueidentifier NOT NULL,
	Version int NOT NULL,
	CreatedBy uniqueidentifier NOT NULL,
	UpdatedBy uniqueidentifier NULL,
	CreatedOn datetime NOT NULL,
	UpdatedOn datetime NULL,
	Name nvarchar(50) NOT NULL,
	FileFormat int NOT NULL,
	PayrollFormatName nvarchar(50) NOT NULL,
	PayrollFormatId uniqueidentifier NOT NULL,
	Minimum datetime NOT NULL,
	Maximum datetime NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL,
	IsDeleted bit NOT NULL
	)  ON [PRIMARY]
GO
IF EXISTS(SELECT * FROM dbo.PayrollExport)
	 EXEC('INSERT INTO dbo.Tmp_PayrollExport (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Name, FileFormat, PayrollFormatName, PayrollFormatId, Minimum, Maximum, BusinessUnit, IsDeleted)
		SELECT Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Id, FileFormat, PayrollFormatName, PayrollFormatId, Minimum, Maximum, BusinessUnit, IsDeleted FROM dbo.PayrollExport WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.PersonsInPayrollExport
	DROP CONSTRAINT FK_PersonsInPayrollExport_PersonID
GO
DROP TABLE dbo.PayrollExport
GO
EXECUTE sp_rename N'dbo.Tmp_PayrollExport', N'PayrollExport', 'OBJECT' 
GO
ALTER TABLE dbo.PayrollExport ADD CONSTRAINT
	PK__PayrollExport__03DB89B3 PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.PayrollExport ADD CONSTRAINT
	FK_PayrollExport_Person_CreatedBy FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollExport ADD CONSTRAINT
	FK_PayrollExport_Person_UpdatedBy FOREIGN KEY
	(
	UpdatedBy
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollExport ADD CONSTRAINT
	FK_PayrollExport_BusinessUnit FOREIGN KEY
	(
	BusinessUnit
	) REFERENCES dbo.BusinessUnit
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

GO
ALTER TABLE dbo.PersonsInPayrollExport ADD CONSTRAINT
	FK_PersonsInPayrollExport_PersonID FOREIGN KEY
	(
	PersonId
	) REFERENCES dbo.PayrollExport
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

--Name: tamasb
--Date: 2009-03-20 11:41 
--Desc: Make BusinessUnit foreign key nullable in ApplicationRole table.
--Expl: NULL foreign keyy will be made for the application roles where there are 
--      independent from any BusinessUnit >>> built in super roles 
---------------- 
-- STEP 1/3 drop foreign key
ALTER TABLE dbo.ApplicationRole
	DROP CONSTRAINT FK_ApplicationRole_BusinessUnit
GO

-- STEP 2/3 make column nullable
ALTER TABLE dbo.ApplicationRole 
	ALTER COLUMN BusinessUnit [uniqueidentifier] NULL

-- STEP 3/3 create and add foreign key
GO
ALTER TABLE dbo.ApplicationRole ADD CONSTRAINT
	FK_ApplicationRole_BusinessUnit FOREIGN KEY
	(
	BusinessUnit
	) REFERENCES dbo.BusinessUnit
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

--Name: tamasb
--Date: 2009-03-20 12:51
--Desc: Add a not nullable BuiltIn column to Person
--Expl: This column will contains the information for a built in super user
---------------- 
-- STEP 1/3 add column nullable
ALTER TABLE dbo.Person ADD
	BuiltIn bit NULL
GO

-- STEP 1/2 add data
Update dbo.Person
	Set BuiltIn = 0
GO

-- STEP 1/3 change column to not nullable
ALTER TABLE dbo.Person 
	ALTER COLUMN BuiltIn bit NOT NULL
GO

--Name: tamasb
--Date: 2009-03-20 12:51
--Desc: Set all role not inbuilt in ApplicationRole table
--Expl: This column will contains the information for a built in super role
---------------- 
Update dbo.ApplicationRole
	Set BuiltIn = 0
GO

----------------  
--Name: ZoÎ Trender
--Date: 2009-03-25 
--Desc: Making datamart and databaseconverter invisible.
----------------
SET NOCOUNT ON
UPDATE person
SET builtin = 1
WHERE applicationlogonname = 'datamart'

UPDATE person
SET builtin = 1
WHERE applicationlogonname = 'DatabaseConverter'

SET NOCOUNT OFF
GO 
GO 
 
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_stg_queue]'))
DROP VIEW [mart].[v_stg_queue]
GO
CREATE VIEW [mart].[v_stg_queue]
AS
SELECT * FROM stage.stg_queue

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
 
/****** Object:  StoredProcedure [mart].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_v_stg_queue_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_v_stg_queue_delete]
GO
/****** Object:  StoredProcedure [mart].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2009-02-02>
-- Description:	<Delete data from v_stg_queue>
-- =============================================
CREATE PROCEDURE [mart].[raptor_v_stg_queue_delete]

AS
BEGIN
	DELETE FROM mart.v_stg_queue
END
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_statistics_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_statistics_load]
GO
-- =============================================
-- Author:		Unknown
-- Create date: 2008-xx-xx
-- Description:	Return the queue workload used in "prepare workload"
-- Change date:	2008-12-02
--				DJ: Use existing functions to split input strings
-- =============================================
CREATE PROCEDURE [mart].[raptor_statistics_load] 
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
	SELECT * FROM mart.SplitStringInt(@QueueList)

	INSERT INTO @TempDateFromList
	SELECT * FROM mart.SplitStringString(@DateFromList)

	INSERT INTO @TempDateToList
	SELECT * FROM mart.SplitStringString(@DateToList)

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
	FROM		mart.fact_queue ql 
	INNER JOIN	mart.dim_date d
		ON ql.date_id = d.date_id 
	INNER JOIN	mart.dim_interval i
		ON ql.interval_id = i.interval_id 
	INNER JOIN	mart.dim_queue q
		ON ql.queue_id = q.queue_id 
	WHERE q.queue_code IN (SELECT QueueID FROM @TempList)
	AND EXISTS
			(SELECT * FROM @TempFromToDates 
			WHERE DATEADD(mi, DATEDIFF(mi,'1900-01-01',i.interval_start), d.date_date) BETWEEN DateFrom and DateTo)
END
GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_reports_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_reports_load]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty list fo reports. E.g fake the Analytics reports
-- Change:		
-- =============================================

CREATE PROCEDURE [mart].[raptor_reports_load]
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
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_queues]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_queues]
GO

CREATE PROCEDURE mart.[raptor_load_queues] 
                            AS
                            BEGIN
	                            SET NOCOUNT ON;
                                SELECT	queue_agg_id QueueAggId, 
		                                queue_code CtiQueueId, 
		                                queue_name [Name],
                                        queue_name [Description], 
		                                datasource_id DataSourceId,
                                        log_object_name DataSource
                                FROM mart.dim_queue WHERE queue_id > -1
                            END
GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_acd_logins]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_acd_logins]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty result set E.g fake the Analytics ACD-logins syncing
-- Change:		
-- =============================================

CREATE PROCEDURE mart.[raptor_load_acd_logins] 
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
 
/****** Object:  StoredProcedure [mart].[raptor_fact_queue_load]    Script Date: 02/02/2009 14:00:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_fact_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_fact_queue_load]
GO
/****** Object:  StoredProcedure [mart].[raptor_fact_queue_load]    Script Date: 02/02/2009 14:00:48 ******/
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
CREATE PROCEDURE [mart].[raptor_fact_queue_load] 
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
	mart.v_stg_queue
 

SET	@min_date = convert(smalldatetime,floor(convert(decimal(18,4),@min_date )))
SET @max_date	= convert(smalldatetime,floor(convert(decimal(18,4),@max_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @min_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @max_date = date_date)

--ANALYZE AND UPDATE DATA IN TEMPORARY TABLE
SELECT *
INTO #stg_queue
FROM mart.v_stg_queue

UPDATE #stg_queue
SET queue_code = d.queue_code
FROM mart.dim_queue d
INNER JOIN #stg_queue stg ON stg.queue_name=d.queue_name
AND d.datasource_id = @datasource_id  
WHERE (stg.queue_code is null OR stg.queue_code='')

ALTER TABLE  #stg_queue ADD interval_id smallint

UPDATE #stg_queue
SET interval_id= i.interval_id
FROM mart.dim_interval i
INNER JOIN #stg_queue stg ON stg.interval=LEFT(i.interval_name,5)

-- Delete rows for the queues imported from file
DELETE FROM mart.fact_queue  
WHERE local_date_id BETWEEN @start_date_id 
AND @end_date_id AND datasource_id = @datasource_id
AND queue_id IN (SELECT queue_id from mart.dim_queue dq INNER JOIN #stg_queue stg ON dq.queue_code=stg.queue_code WHERE dq.datasource_id = @datasource_id )


INSERT INTO mart.fact_queue
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
	mart.dim_date		d
ON
	stg.date	= d.date_date
JOIN 
	mart.dim_interval i
ON 
	i.interval_id=stg.interval_id
JOIN
	mart.dim_queue		q
ON
	q.queue_code= stg.queue_code 
	AND q.datasource_id = @datasource_id

END  
GO  
 
/****** Object:  StoredProcedure [mart].[raptor_dim_queue_load]    Script Date: 02/02/2009 14:00:15 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_dim_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_dim_queue_load]
GO
/****** Object:  StoredProcedure [mart].[raptor_dim_queue_load]    Script Date: 02/02/2009 14:00:09 ******/
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
CREATE PROCEDURE [mart].[raptor_dim_queue_load] 
	
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
SET IDENTITY_INSERT mart.dim_queue ON
INSERT INTO mart.dim_queue
	(
	queue_id,
	queue_name,
	datasource_id	
	)
SELECT 
	queue_id			=-1,
	queue_name			='Not Defined',
	datasource_id		=-1
WHERE NOT EXISTS (SELECT * FROM mart.dim_queue where queue_id = -1)
SET IDENTITY_INSERT mart.dim_queue OFF

--Update
--Existing queues with a queue_code in the importfile
UPDATE mart.dim_queue
SET
	queue_code		=stage.queue_code, 
	queue_name		=stage.queue_name,
	log_object_name =@log_object_name, 
	update_date		= getdate()
FROM
	mart.v_stg_queue stage
JOIN
	mart.dim_queue
ON
	mart.dim_queue.queue_code		= stage.queue_code 			AND
	mart.dim_queue.datasource_id	= @datasource_id
WHERE stage.queue_code IS NOT NULL

--------------------------------------------------------------
--Update
--Existing queues without a queue_code (fallback on queue_name)
UPDATE mart.dim_queue
SET 
	queue_code		=q.queue_id, 
	queue_name		=stage.queue_name,
	log_object_name =@log_object_name, 
	update_date		= getdate()
FROM
	mart.v_stg_queue stage	
JOIN
	mart.dim_queue q
ON
	q.queue_name		= stage.queue_name 			AND
	q.datasource_id		= @datasource_id
WHERE stage.queue_code IS NULL

---------------------------------------------------------------
-- Reset identity seed.
DECLARE @max_id INT
SET @max_id= (SELECT max(queue_id) FROM mart.dim_queue)

DBCC CHECKIDENT ('mart.dim_queue',reseed,@max_id);
---------------------------------------------------------------------------
-- Insert new queues with a queue_code
INSERT INTO mart.dim_queue
	( 
	queue_agg_id,
	queue_code, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_code				= stage.queue_code, 
	queue_name				= max(stage.queue_name),
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	mart.v_stg_queue stage
WHERE 
	NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
					WHERE	d.queue_code= stage.queue_code 	AND
							d.datasource_id =@datasource_id
				)
AND stage.queue_code IS NOT NULL
GROUP BY stage.queue_code

----------------------------------------------------------------------------------------
-- Insert new queues without a queue_code (fallback on queue_name)
INSERT INTO mart.dim_queue
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
	queue_name				= stage.queue_name,
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	mart.v_stg_queue stage
WHERE 
	NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
					WHERE	d.queue_name= stage.queue_name 	AND
							d.datasource_id =@datasource_id
				)
AND stage.queue_code IS NULL
GROUP BY stage.queue_name

--SET queue_agg_id AND queue_code TO SAME VALUES AS queue_id IF NO queue_code OR queue_agg_id
UPDATE mart.dim_queue
SET queue_agg_id=queue_id, queue_code= queue_id
WHERE queue_agg_id IS NULL 
AND queue_code IS NULL
AND datasource_id=@datasource_id

UPDATE mart.dim_queue
SET queue_agg_id=queue_code
WHERE queue_agg_id IS NULL 
AND queue_code IS NOT NULL
AND datasource_id=@datasource_id


END
GO
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_dim_queue_create]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_dim_queue_create]
GO

CREATE PROCEDURE [mart].[raptor_dim_queue_create] 
@queue_code nvarchar(50),
@queue_name nvarchar(50)

	
AS

insert into mart.dim_queue(queue_code, queue_name, log_object_name, queue_agg_id, datasource_id)
values(@queue_code,  @queue_name, 'importfile', 20, 1)
select @@identity   
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_dim_date_getId_ByDate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_dim_date_getId_ByDate]
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

CREATE PROCEDURE [mart].[raptor_dim_date_getId_ByDate] 
@DateList varchar(8000)
AS

DECLARE	@TempDateList table
(
Date_date smallDateTime
)


--Get string into temp table
INSERT INTO @TempDateList
SELECT * FROM mart.SplitStringString(@DateList)

--Return result set
SELECT	ISNULL(d.date_id,-1) as date_id,
		temp.date_date as date_date
FROM @TempDateList temp
LEFT OUTER JOIN mart.dim_date d
ON temp.Date_date = d.date_date
GO
  

----------------
--Name: tamasb
--Date: 2009-03-20 17:13
--Desc: Add superxxx data
--Expl: Super User, Super Role
---------------- 
SET NOCOUNT ON
BEGIN
			--declare
			DECLARE @SuperUserId as uniqueidentifier
			DECLARE @SuperRoleId as uniqueidentifier
			DECLARE @AllFunctionId as uniqueidentifier

			--init
			SELECT	  @SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'
			SELECT	  @SuperRoleId = '193AD35C-7735-44D7-AC0C-B8EDA0011E5F'
			
			--insert to super user
			IF  (
				NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId) -- check for the existence of super user role
				)
			BEGIN
				INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
				VALUES			     (@SuperUserId,1,@SuperUserId, NULL, getdate(), NULL, '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 

			END
			IF  (
				NOT EXISTS (SELECT	Id FROM ApplicationFunction WHERE FunctionCode = 'All' and IsDeleted = 0)
				)
			BEGIN
				INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [SortOrder], [IsDeleted])
				VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, NULL, 'All', 'xxAll', '0000-1000', 'Raptor', NULL, 0) 

			END
			
			SELECT	  @AllFunctionId = Id FROM ApplicationFunction WHERE FunctionCode = 'All' and IsDeleted = 0

			--insert to super role
			IF  (NOT EXISTS (SELECT id FROM [dbo].[ApplicationRole] WHERE Id = @SuperRoleId)) -- check for the existence of super user role
				INSERT [dbo].[ApplicationRole]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Name], [DescriptionText], [BuiltIn], [BusinessUnit], [IsDeleted])
				VALUES (@SuperRoleId,1,@SuperUserId, NULL, getdate(), NULL, '_Super Role', 'xxSuperRole', 1, NULL, 0) 

			--insert into availableData
			IF  (NOT EXISTS (SELECT id FROM [dbo].[AvailableData] WHERE [ApplicationRole] = @SuperRoleId))
				INSERT [dbo].[AvailableData]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [ApplicationRole], [AvailableDataRange], [IsDeleted])
				VALUES (newid(),1,@SuperUserId, NULL, getdate(), NULL, @SuperRoleId, 5, 0) 

			--insert into ApplicationFunctionInRole
			IF  (NOT EXISTS (SELECT * FROM [dbo].[ApplicationFunctionInRole] WHERE [ApplicationRole] = @SuperRoleId AND [ApplicationFunction] = @AllFunctionId))
				INSERT [dbo].[ApplicationFunctionInRole]([ApplicationRole], [ApplicationFunction])
				VALUES (@SuperRoleId, @AllFunctionId) 

			--insert into PersonInApplicationRole
			IF  (NOT EXISTS (SELECT * FROM [dbo].[PersonInApplicationRole] WHERE [Person] = @SuperUserId AND [ApplicationRole] = @SuperRoleId))
				INSERT [dbo].[PersonInApplicationRole]([Person], [ApplicationRole])
				VALUES (@SuperUserId, @SuperRoleId) 


END
SET NOCOUNT OFF
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (82,'7.0.82') 
