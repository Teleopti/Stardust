/* 
Trunk initiated: 
2009-08-27 
15:15
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

----------------  
--Name: Micke
--Date: 2009-08-28
--Desc: Removing listsettings
----------------  


ALTER TABLE [dbo].[ListSettingItem] DROP CONSTRAINT [FK_ListSettingItem_ListSetting]
GO

DROP TABLE [dbo].[ListSettingItem]
GO

ALTER TABLE [dbo].[ListSetting] DROP CONSTRAINT [FK_ListSetting_Setting]
GO

DROP TABLE [dbo].[ListSetting]
GO

----------------  
--Name: Robin Karlsson
--Date: 2009-08-31
--Desc: Changing the behavior of shift trade requests. Now they contain a list of date only periods.
--		Note! This should be removed to release script!
----------------  
create table #str (id uniqueidentifier)
insert into #str
select request from shifttraderequest
insert into #str
select parent from request where id in (select * from #str)
delete from shifttraderequest where request in (select * from #str)
delete from request where id in (select * from #str)
delete from personrequest where id in (select * from #str)
GO

CREATE TABLE [dbo].[ShiftTradeRequestPeriods](
	[ShiftTradeRequest] [uniqueidentifier] NOT NULL,
	[RequestedStartDate] [datetime] NOT NULL,
	[RequestedEndDate] [datetime] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ShiftTradeRequestPeriods]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeRequestPeriods_ShiftTradeRequest] FOREIGN KEY([ShiftTradeRequest])
REFERENCES [dbo].[ShiftTradeRequest] ([Request])
GO
ALTER TABLE [dbo].[ShiftTradeRequestPeriods] CHECK CONSTRAINT [FK_ShiftTradeRequestPeriods_ShiftTradeRequest]
GO
ALTER TABLE dbo.ShiftTradeRequest
	DROP COLUMN RequestedStartDateTime, RequestedEndDateTime
GO
ALTER TABLE ShiftTradeRequest ADD ChecksumOwner int NOT NULL
GO
ALTER TABLE ShiftTradeRequest ADD ChecksumRequested int NOT NULL
GO


----------------  
--Name: Roger Kratz
--Date: 2009-08-31
--Desc: Adding IChangeInfo columns + version
--		Make it easy for myself and dropping the table first. Doesn't really matter if info is lost.
----------------  
DROP TABLE [dbo].[GlobalSettingData]
GO
create table [dbo].[GlobalSettingData] 
(
	Id UNIQUEIDENTIFIER not null, 
	Version INT not null, 
	CreatedBy UNIQUEIDENTIFIER not null, 
	UpdatedBy UNIQUEIDENTIFIER null, 
	CreatedOn DATETIME not null, 
	UpdatedOn DATETIME null, 
	[Key] NVARCHAR(255) not null, 
	SerializedValue VARBINARY(8000) not null, 
	BusinessUnit UNIQUEIDENTIFIER not null
)
ON [PRIMARY]

ALTER TABLE [dbo].[GlobalSettingData] ADD CONSTRAINT [PK_GlobalSettingData] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)
ON [PRIMARY]
	
ALTER TABLE  [dbo].[GlobalSettingData] ADD CONSTRAINT [UQ_Key_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[Key] ASC,
	[BusinessUnit] ASC
)
ON [PRIMARY]

alter table [dbo].[GlobalSettingData] add constraint FK_GlobalSetting_Person_CreatedBy foreign key (CreatedBy) references Person

alter table [dbo].[GlobalSettingData] add constraint FK_GlobalSetting_Person_UpdatedBy foreign key (UpdatedBy) references Person

alter table [dbo].[GlobalSettingData] add constraint FK_GlobalSettingData_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
GO

----------------  
--Name: Robin Karlsson
--Date: 2009-09-01
--Desc: This is just to drop a temporary table created within this trunk. Should not be released!
----------------  
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ShiftTradeRequestPeriods_ShiftTradeRequest]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShiftTradeRequestPeriods]'))
ALTER TABLE [dbo].[ShiftTradeRequestPeriods] DROP CONSTRAINT [FK_ShiftTradeRequestPeriods_ShiftTradeRequest]
GO
/****** Object:  Table [dbo].[ShiftTradeRequestPeriods]    Script Date: 09/01/2009 15:02:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShiftTradeRequestPeriods]') AND type in (N'U'))
DROP TABLE [dbo].[ShiftTradeRequestPeriods]
GO
ALTER TABLE dbo.ShiftTradeRequest DROP COLUMN ChecksumOwner
ALTER TABLE dbo.ShiftTradeRequest DROP COLUMN ChecksumRequested
GO

----------------  
--Name: Robin Karlsson
--Date: 2009-09-01
--Desc: This is the real stuff for now regarding shift trades
---------------- 
ALTER TABLE [dbo].ShiftTradeRequest DROP CONSTRAINT [FK_ShiftTradeRequest_Person]
GO
ALTER TABLE dbo.ShiftTradeRequest DROP COLUMN RequestedPerson
GO

CREATE TABLE [dbo].[ShiftTradeSwapDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[PersonFrom] [uniqueidentifier] NOT NULL,
	[PersonTo] [uniqueidentifier] NOT NULL,
	[DateFrom] [datetime] NOT NULL,
	[DateTo] [datetime] NOT NULL,
	[ChecksumFrom] [int] NOT NULL,
	[ChecksumTo] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeSwapDetail_PersonFrom] FOREIGN KEY([PersonFrom])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail] CHECK CONSTRAINT [FK_ShiftTradeSwapDetail_PersonFrom]
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeSwapDetail_PersonTo] FOREIGN KEY([PersonTo])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail] CHECK CONSTRAINT [FK_ShiftTradeSwapDetail_PersonTo]
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeSwapDetail_ShiftTradeRequest] FOREIGN KEY([Parent])
REFERENCES [dbo].[ShiftTradeRequest] ([Request])
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail] CHECK CONSTRAINT [FK_ShiftTradeSwapDetail_ShiftTradeRequest]
GO

----------------  
--Name: Anders F
--Date: 2009-09-01
--Desc: Make sure orderindex is >=0 to make sure Shifts fail when it tries to mess up my db
----------------  
create table #rs (id uniqueidentifier)

insert into #rs
select distinct a.parent from activityextender a
where a.parent in (select b.parent from activityextender b where orderindex <0)

delete from activityextender where parent in (select id from #rs)
delete from accessibilitydates where ruleset in (select id from #rs)
delete from accessibilitydaysofweek where ruleset in (select id from #rs)
delete from activitytimelimiter where parent in (select id from #rs)
delete from contracttimelimiter where parent in (select id from #rs)
delete from rulesetrulesetbag where ruleset in (select id from #rs)
delete from workshiftruleset where id in (select id from #rs)

drop table #rs
ALTER TABLE ActivityExtender ADD CONSTRAINT CK_ActivityExtender_OrderIndex
    CHECK (OrderIndex >= 0)
GO


----------------  
--Name: Roger Kratz
--Date: 2009-09-08
--Desc: Dropping pk in kopplingstabeller 
--REMARK: David! Don't know how you want these scripts?
-- Are we...
--    * ...sure that these PK names are valid in every installation?
--    * ...should if exists be used?
----------------  

alter table AvailableTeamsInApplicationRole drop constraint PK_AvailableTeamsInApplicationRole
alter table AvailableTeamsInApplicationRole drop column collection_id

alter table AvailableUnitsInApplicationRole drop constraint PK_AvailableUnitsInApplicationRole
alter table AvailableUnitsInApplicationRole drop column collection_id

alter table AvailableSitesInApplicationRole drop constraint PK_AvailableSitesInApplicationRole
alter table AvailableSitesInApplicationRole drop column collection_id

alter table AvailablePersonsInApplicationRole drop constraint PK_AvailablePersonsInApplicationRole
alter table AvailablePersonsInApplicationRole drop column collection_id
GO

----------------  
--Name: Micke D
--Date: 2009-09-08
--Desc: Dropping of old tables used by old settings
----------------  

DROP TABLE [dbo].[PersonalSettingData]
GO
--- Create with varbinary(max)
CREATE TABLE [dbo].[PersonalSettingData](
	[Id] [uniqueidentifier] NOT NULL,
	[Key] [nvarchar](255) NOT NULL,
	[SerializedValue] [varbinary](max) NOT NULL,
	[OwnerPerson] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonalSettingData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Key_OwnerPerson] UNIQUE NONCLUSTERED 
(
	[Key] ASC,
	[OwnerPerson] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CommonNameDescriptionSetting] DROP CONSTRAINT [FK_CommonNameDescriptionSetting_Setting]
GO

DROP TABLE [dbo].[CommonNameDescriptionSetting]
GO

ALTER TABLE [dbo].[CommonSettings] DROP CONSTRAINT [FK_CommonSettings_BusinessUnit]
GO

ALTER TABLE [dbo].[CommonSettings] DROP CONSTRAINT [FK_CommonSettings_Person_CreatedBy]
GO

ALTER TABLE [dbo].[CommonSettings] DROP CONSTRAINT [FK_CommonSettings_Person_UpdatedBy]
GO

DROP TABLE [dbo].[CommonSettings]
GO

ALTER TABLE [dbo].[Setting] DROP CONSTRAINT [FK_Setting_Person]
GO

ALTER TABLE [dbo].[Setting] DROP CONSTRAINT [FK_Setting_SettingCategory]
GO

DROP TABLE [dbo].[Setting]
GO

ALTER TABLE [dbo].[SettingCategory] DROP CONSTRAINT [FK_SettingCategory_BusinessUnit]
GO

ALTER TABLE [dbo].[SettingCategory] DROP CONSTRAINT [FK_SettingCategory_Person_CreatedBy]
GO

ALTER TABLE [dbo].[SettingCategory] DROP CONSTRAINT [FK_SettingCategory_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[SettingCategory] DROP CONSTRAINT [FK_SettingCategory_SettingCategory]
GO

DROP TABLE [dbo].[SettingCategory]
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
	WHERE q.queue_original_id IN (SELECT QueueID FROM @TempList)
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
                                SELECT	
										queue_original_id	QueueOriginalId, 
										queue_agg_id		QueueAggId, 
		                                queue_id			QueueMartId,
		                                datasource_id		DataSourceId,
		                                log_object_name		LogObjectName,
                                        queue_name			Name,
                                        queue_Description	[Description]                                        
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
	[acd_login_original_id] [nvarchar](50) NULL,
	[acd_login_name] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[is_active] [bit] NULL,
	[datasource_id] [smallint] NULL
)

--Select empty result set for Freemimum
SELECT	acd_login_id			AcdLogOnMartId,
		acd_login_agg_id		AcdLogOnAggId, 
		acd_login_original_id	AcdLogOnOriginalId, 
		acd_login_name			AcdLogOnName,
		is_active				Active,
		datasource_id			DataSourceId
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
SET queue_code = d.queue_original_id
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
AND queue_id IN (SELECT queue_id from mart.dim_queue dq INNER JOIN #stg_queue stg ON dq.queue_original_id=stg.queue_code WHERE dq.datasource_id = @datasource_id )


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
	stg.interval = substring(i.interval_name,1,5)
JOIN
	mart.dim_queue		q
ON
	q.queue_original_id= stg.queue_code 
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
-- Create date: 2009-02-02
-- Description:	File Import - Loads data to dim_queue from stg_queue
-- Update date: 2009-07-07 Update queue_description if NULL
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
--Existing queues with a queue_original_id in the importfile
UPDATE mart.dim_queue
SET
	queue_original_id		=stage.queue_code, 
	queue_name		=stage.queue_name,
	log_object_name =@log_object_name, 
	update_date		= getdate()
FROM
	mart.v_stg_queue stage
JOIN
	mart.dim_queue
ON
	mart.dim_queue.queue_original_id		= stage.queue_code 			AND
	mart.dim_queue.datasource_id	= @datasource_id
WHERE stage.queue_code IS NOT NULL

--------------------------------------------------------------
--Update
--Existing queues without a queue_code (fallback on queue_name)
UPDATE mart.dim_queue
SET 
	queue_original_id		=q.queue_id, 
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
	queue_original_id, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_original_id		= stage.queue_code, 
	queue_name				= max(stage.queue_name),
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	mart.v_stg_queue stage
WHERE 
	NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
					WHERE	d.queue_original_id= stage.queue_code 	AND
							d.datasource_id =@datasource_id
				)
AND stage.queue_code IS NOT NULL
GROUP BY stage.queue_code

----------------------------------------------------------------------------------------
-- Insert new queues without a queue_code (fallback on queue_name)
INSERT INTO mart.dim_queue
	( 
	queue_agg_id,
	queue_original_id, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_original_id				= NULL, 
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
SET queue_agg_id=queue_id, queue_original_id= queue_id
WHERE queue_agg_id IS NULL 
AND queue_original_id IS NULL
AND datasource_id=@datasource_id

UPDATE mart.dim_queue
SET queue_agg_id=queue_original_id
WHERE queue_agg_id IS NULL 
AND queue_original_id IS NOT NULL
AND datasource_id=@datasource_id

--Update queue_description if IS NULL
UPDATE mart.dim_queue
SET queue_description = queue_name
WHERE queue_description IS NULL
AND datasource_id=@datasource_id --only for File Imports!

END
GO
  
GO  
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (145,'7.0.145') 
