/* 
Trunk initiated: 
2009-06-29 
09:40
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 

--Name: tamasb  
--Date: 2009-07-01  
--Desc: Part of the application function sync-stop task. We do not need this column.  

ALTER TABLE [dbo].[ApplicationFunction] DROP COLUMN [SortOrder]
GO

----------------  
--Name: David Jonsson
--Date: 2009-07-05
--Desc: Obosolete SPs
--		Remember to remove from them from Source Control! Else they will be built and delivered again
----------------  
DROP PROCEDURE [mart].[raptor_dim_queue_create]
GO
DROP PROCEDURE [mart].[raptor_dim_date_getId_ByDate]
GO

----------------  
--Name: David Jonsson
--Date: 2009-07-05
--Desc: Bug in application
--		Duplicated keys in Request, Clean up and Create unique constrataint on Parent
----------------  
DECLARE @Parent AS UniqueIdentifier
DECLARE @numDups INT

SET NOCOUNT ON

--DELETE Orphane Request
DELETE FROM Request
WHERE Id NOT IN (SELECT Request FROM AbsenceRequest)
AND  Id NOT IN (SELECT Request FROM ShiftTradeRequest)
AND  Id NOT IN (SELECT Request FROM TextRequest)

--DELETE Orphane PersonRequest
DELETE FROM PersonRequest WHERE Id NOT IN (SELECT Parent FROM Request)

--Take care of dublicates
DECLARE dupsCsr CURSOR FOR
	SELECT Parent,COUNT(Parent)
	FROM Request
	GROUP BY Parent
	HAVING COUNT(Parent) > 1

OPEN dupsCsr
FETCH NEXT FROM dupsCsr INTO @Parent,@numDups 
WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT 'Duplicates Request found! Deleting ...'
	
	SET @numDups = @numDups - 1 --delete all but 1 of the duplicates
	SET ROWCOUNT @numDups

	--Delete Child tables
	DELETE TextRequest
	FROM TextRequest
	INNER JOIN Request
	ON TextRequest.Request = Request.Id
	WHERE Request.Parent = @Parent
	
	DELETE ShiftTradeRequest
	FROM ShiftTradeRequest
	INNER JOIN Request
	ON ShiftTradeRequest.Request = Request.Id
	WHERE Request.Parent = @Parent
		
	DELETE AbsenceRequest
	FROM AbsenceRequest
	INNER JOIN Request
	ON AbsenceRequest.Request = Request.Id
	WHERE Request.Parent = @Parent
	
	--Delete Main table
	DELETE FROM Request	WHERE Parent = @Parent
	
	SET ROWCOUNT 0

	--Get next duplicate
	FETCH NEXT FROM dupsCsr INTO @Parent,@numDups 
	
	PRINT 'Done!'
END --WHILE
CLOSE dupsCsr
DEALLOCATE dupsCsr

--Add a Constraint to prevent this from happening again
--IF NOT EXISTS used because we deployed this as a add-hoc patch for a few customers
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Request]') AND name = N'UQ_Request_Parent')
	ALTER TABLE [dbo].[Request] ADD CONSTRAINT 
	UQ_Request_Parent UNIQUE NONCLUSTERED
	(
		Parent
	) ON [PRIMARY]

----------------  
--Name: David Jonsson
--Date: 2009-06-30
--Desc: Re-factor of dim_queue in Analytics
--		Remove Raptor Sync of Queues and ACD-logon
----------------  
SET NOCOUNT ON
GO

EXEC dbo.sp_rename
	@objname=N'[dbo].[ExternalLogOn].[LogOnId]',
	@newname=N'AcdLogOnMartId',
	@objtype=N'COLUMN'
GO
EXEC dbo.sp_rename
	@objname=N'[dbo].[ExternalLogOn].[LogOnAggId]',
	@newname=N'AcdLogOnAggId',
	@objtype=N'COLUMN'
GO
EXEC dbo.sp_rename
	@objname=N'[dbo].[ExternalLogOn].[LogOnCode]',
	@newname=N'AcdLogOnOriginalId',
	@objtype=N'COLUMN'
GO
EXEC dbo.sp_rename
	@objname=N'[dbo].[ExternalLogOn].[LogOnName]',
	@newname=N'AcdLogOnName',
	@objtype=N'COLUMN'
GO

--Queue re-fac
EXEC dbo.sp_rename
	@objname=N'[dbo].[QueueSource].[CtiQueueId]',
	@newname=N'QueueOriginalId',
	@objtype=N'COLUMN'
GO
EXEC dbo.sp_rename
	@objname=N'[dbo].[QueueSource].[DataSource]',
	@newname=N'LogObjectName',
	@objtype=N'COLUMN'
GO

----------------  
--Name: David Jonsson
--Date: 2009-06-30
--Desc: Analytics will from now own more Columns in QueueSource.
--		Syncing from appliation will be removed
--		Add new column [QueueMartId] as External Key used by ETL to update QueueSource
----------------  
ALTER TABLE dbo.QueueSource
	DROP CONSTRAINT FK_QueueSource_Person_CreatedBy
GO
ALTER TABLE dbo.QueueSource
	DROP CONSTRAINT FK_QueueSource_Person_UpdatedBy
GO
CREATE TABLE dbo.Tmp_QueueSource
	(
	Id uniqueidentifier NOT NULL,
	Version int NOT NULL,
	CreatedBy uniqueidentifier NOT NULL,
	UpdatedBy uniqueidentifier NULL,
	CreatedOn datetime NOT NULL,
	UpdatedOn datetime NULL,
	QueueMartId int NOT NULL,
	QueueAggId int NOT NULL,
	QueueOriginalId int NOT NULL,
	DataSourceId int NULL,
	LogObjectName nvarchar(50) NULL,
	Name nvarchar(50) NOT NULL,
	Description nvarchar(50) NULL,
	IsDeleted bit NOT NULL
	)  ON [PRIMARY]
GO

--Restore data	
INSERT INTO dbo.Tmp_QueueSource (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, QueueOriginalId, DataSourceId, QueueAggId, LogObjectName, Name, Description, IsDeleted,QueueMartId)
SELECT Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, QueueOriginalId, DataSourceId, QueueAggId, LogObjectName, Name, Description, IsDeleted, -1 FROM dbo.QueueSource WITH (HOLDLOCK TABLOCKX)

GO
ALTER TABLE dbo.QueueSourceCollection
	DROP CONSTRAINT FK_QueueSourceCollection_Workload
GO
DROP TABLE dbo.QueueSource
GO
EXECUTE sp_rename N'dbo.Tmp_QueueSource', N'QueueSource', 'OBJECT' 
GO
ALTER TABLE dbo.QueueSource ADD CONSTRAINT
	PK_QueueSource PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.QueueSource ADD CONSTRAINT
	FK_QueueSource_Person_CreatedBy FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.QueueSource ADD CONSTRAINT
	FK_QueueSource_Person_UpdatedBy FOREIGN KEY
	(
	UpdatedBy
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.QueueSourceCollection ADD CONSTRAINT
	FK_QueueSourceCollection_Workload FOREIGN KEY
	(
	QueueSource
	) REFERENCES dbo.QueueSource
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO

----------------  
--Name: Jonas Nordh
--Date: 2009-06-30
--Desc: Rename one column and add one column to Raptor´s table [mart].[dim_queue]
----------------  
EXEC dbo.sp_rename
	@objname=N'[mart].[dim_queue].[queue_code]',
	@newname=N'queue_original_id',
	@objtype=N'COLUMN'
GO

--Add new column: [queue_description]
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_queue_name
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_datasource_id
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_insert_date
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_update_date
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_datasource_update_date
GO
CREATE TABLE mart.Tmp_dim_queue
	(
	queue_id int NOT NULL IDENTITY (1, 1),
	queue_agg_id int NULL,
	queue_original_id nvarchar(50) NULL,
	queue_name nvarchar(100) NOT NULL,
	queue_description nvarchar(100) NULL,
	log_object_name nvarchar(100) NULL,
	datasource_id smallint NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL,
	datasource_update_date smalldatetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_queue_name DEFAULT ('Not Defined') FOR queue_name
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_datasource_id DEFAULT ((-1)) FOR datasource_id
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_update_date DEFAULT (getdate()) FOR update_date
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_datasource_update_date DEFAULT ('1900-01-01') FOR datasource_update_date
GO
SET IDENTITY_INSERT mart.Tmp_dim_queue ON
GO
INSERT INTO mart.Tmp_dim_queue (queue_id, queue_agg_id, queue_original_id, queue_name, log_object_name, datasource_id, insert_date, update_date, datasource_update_date)
SELECT queue_id, queue_agg_id, queue_original_id, queue_name, log_object_name, datasource_id, insert_date, update_date, datasource_update_date FROM mart.dim_queue WITH (HOLDLOCK TABLOCKX)
GO
SET IDENTITY_INSERT mart.Tmp_dim_queue OFF
GO
ALTER TABLE mart.fact_queue
	DROP CONSTRAINT FK_fact_queue_dim_queue
GO
DROP TABLE mart.dim_queue
GO
EXECUTE sp_rename N'mart.Tmp_dim_queue', N'dim_queue', 'OBJECT' 
GO
ALTER TABLE mart.dim_queue ADD CONSTRAINT
	PK_dim_queue PRIMARY KEY CLUSTERED 
	(
	queue_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_dim_queue ON mart.dim_queue
	(
	queue_original_id,
	queue_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE mart.fact_queue ADD CONSTRAINT
	FK_fact_queue_dim_queue FOREIGN KEY
	(
	queue_id
	) REFERENCES mart.dim_queue
	(
	queue_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

--Kör detta script i TeleoptiCCC7 innan kö-synkningskoden installeras:
UPDATE QueueSource
SET QueueOriginalId = 0,
           QueueMartId = 0,
           DataSourceId = 0,
           LogObjectName = '',
           Name = qs.QueueAggId,
           Description = qs.QueueAggId
FROM QueueSource qs


--Kör detta script i TeleoptiCCC7 innan agentlogin-synkningskoden installeras:
-- Delete all agent logins not mapped to a person
DELETE FROM ExternalLogOn
WHERE Id NOT IN
( 
           SELECT el.id 
           FROM ExternalLogOn el
           INNER JOIN 
                      ExternalLogOnCollection elc
           ON el.id = elc.ExternalLogOn
)

-- Reset all agent logins back to the state equals to after db convert/upgrade
UPDATE ExternalLogOn
SET AcdLogOnMartId = -1,
           AcdLogOnOriginalId = el.AcdLogOnAggId,
           AcdLogOnName= el.AcdLogOnAggId,
           DataSourceId = -1
FROM ExternalLogOn el
GO

----------------  
--Name: Robin Karlsson
--Date: 2009-08-04
--Desc: Remove not null constraint on column to enable alarms when agent is out of adherence by logging out
----------------  
ALTER TABLE StateGroupActivityAlarm ALTER COLUMN StateGroup uniqueidentifier NULL
GO

----------------  
--Name: Robin Karlsson
--Date: 2009-08-07
--Desc: Remove the Constraint as the domain/nhib handles this now
----------------  
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Request]') AND name = N'UQ_Request_Parent')
	ALTER TABLE [dbo].[Request] DROP CONSTRAINT UQ_Request_Parent
GO

----------------  
--Name: Robin Karlsson
--Date: 2009-08-07  
--Desc: Moving chart settings to it's own root
----------------  
EXECUTE sp_rename N'dbo.ChartSetting.Setting', N'Id', 'COLUMN' 
GO
DELETE FROM dbo.ChartSeriesSetting
GO
DELETE FROM dbo.ChartSetting
GO
ALTER TABLE dbo.ChartSetting ADD
	CreatedBy uniqueidentifier NOT NULL,
	UpdatedBy uniqueidentifier NULL,
	CreatedOn datetime NOT NULL,
	UpdatedOn datetime NULL,
	ChartSettingName nvarchar(50) NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL
GO

----------------  
--Name: Robin Karlsson
--Date: 2009-08-07
--Desc: Adding separate settings for intraday
----------------  
CREATE TABLE [dbo].[IntradaySetting](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DockingState] [nvarchar](1024) NULL,
	[ChartSetting] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[IntradaySetting]  WITH CHECK ADD  CONSTRAINT [FK_IntradaySetting_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[IntradaySetting] CHECK CONSTRAINT [FK_IntradaySetting_BusinessUnit]
GO
ALTER TABLE [dbo].[IntradaySetting]  WITH CHECK ADD  CONSTRAINT [FK_IntradaySetting_ChartSetting] FOREIGN KEY([ChartSetting])
REFERENCES [dbo].[ChartSetting] ([Id])
GO
ALTER TABLE [dbo].[IntradaySetting] CHECK CONSTRAINT [FK_IntradaySetting_ChartSetting]
GO
ALTER TABLE [dbo].[IntradaySetting]  WITH CHECK ADD  CONSTRAINT [FK_IntradaySetting_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[IntradaySetting] CHECK CONSTRAINT [FK_IntradaySetting_Person_CreatedBy]
GO
ALTER TABLE [dbo].[IntradaySetting]  WITH CHECK ADD  CONSTRAINT [FK_IntradaySetting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[IntradaySetting] CHECK CONSTRAINT [FK_IntradaySetting_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ChartSetting] DROP CONSTRAINT [FK_ChartSetting_Setting]
GO

----------------  
--Name: Zoë Trender
--Date: 2009-08-10
--Desc: Updating ApplicationRoles so that only SuperRole is BuiltIn
----------------
UPDATE ApplicationRole
SET BuiltIn = 0
WHERE Id <> '193AD35C-7735-44D7-AC0C-B8EDA0011E5F'
GO
UPDATE ApplicationRole
SET BuiltIn = 1
WHERE Id = '193AD35C-7735-44D7-AC0C-B8EDA0011E5F'
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
-- returnerar en tabell istÃ¤llet
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
-- returnerar en tabell istÃ¤llet
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

--tidszoner, hur g”ra?
--DECLARE @time_zone_id smallint
--SELECT 
--	@time_zone_id = ds.time_zone_id
--FROM
--	v_sys_datasource ds
--WHERE 
--	ds.datasource_id= @datasource_id

--CREATE TABLE #bridge_time_zone(date_id int,interval_id int,time_zone_id int,local_date_id int,local_interval_id int)
--
----HÇÏmta datum och intervall grupperade sÇ¾ vi slipper dubletter vid sommar-vintertid
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
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (134,'7.0.134') 
