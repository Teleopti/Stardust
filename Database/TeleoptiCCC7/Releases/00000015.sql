/* 
BuildTime is: 
2008-12-09 
16:05
*/ 
/* 
Trunk initiated: 
2008-12-03 
12:15
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Micke D  
--Date: 2008-12-03  
--Desc: Added JusticeValueWindow system setting 
----------------  
INSERT INTO Setting(Id, Name, SerializedValue, TypeName, Parent)
SELECT NEWID(), 'JusticePointWindow', '<?xml version="1.0" encoding="utf-8"?><int>90</int>', 'System.Int32', Id
FROM SettingCategory
WHERE [Name] = 'RaptorSettings'

----------------  
--Name: Claes S  
--Date: 2008-12-05  
--Desc: PersonDayOff: Added DayOff Column and inserted values, Renamed Column Anchor, Removed Value, Removed TargetLength 
----------------  
EXEC sp_rename 'dbo.PersonDayOff.Anchor', DateTimeAnchor, 'COLUMN'
GO

ALTER TABLE dbo.PersonDayOff
DROP COLUMN TargetLength
GO

ALTER TABLE dbo.PersonDayOff
DROP COLUMN [Value]
GO

DECLARE @DBConverter uniqueidentifier
SELECT @DBConverter = Id 
FROM dbo.Person
WHERE ApplicationLogOnName = 'DatabaseConverter'

INSERT INTO dbo.DayOff(Id, Version, CreatedBy, CreatedOn, [Name], ShortName, Flexibility, Anchor, TargetLength, BusinessUnit, IsDeleted)
SELECT  NEWID(), 1, @DBConverter, GETDATE(),'DayOff', 'DO', 0, 432000000000, 864000000000, Id, 0
FROM dbo.BusinessUnit
GO
	
ALTER TABLE dbo.PersonDayOff ADD
	DayOff uniqueidentifier NULL
GO

UPDATE dbo.PersonDayOff 
SET DayOff = do.Id
FROM dbo.DayOff AS do
INNER JOIN dbo.PersonDayOff AS pdo ON do.BusinessUnit = pdo.BusinessUnit
GO

ALTER TABLE dbo.PersonDayOFF
ALTER COLUMN DayOff uniqueidentifier NOT NULL							
GO

ALTER TABLE [dbo].[PersonDayOff]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_DayOff] FOREIGN KEY([DayOff])
REFERENCES [dbo].[DayOff] ([Id])
GO

ALTER TABLE [dbo].[PersonDayOff] CHECK CONSTRAINT [FK_PersonDayOff_DayOff]
GO

----------------  
--Name: Dinesh R 
--Date: 2008-12-05  
--Desc: Added Requested period to ShiftTradeRequest
----------------  
ALTER TABLE ShiftTradeRequest ADD [RequestedStartDateTime] [datetime] NULL
ALTER TABLE ShiftTradeRequest ADD [RequestedEndDateTime] [datetime] NULL
GO
----------------  
--Name: MadhurangaP  
--Date: 2008-12-07  
--Desc: Add Schedule preference date to Team  
----------------  
ALTER TABLE Team ADD [SchedulePreferenceDate] [datetime] NULL
GO
----------------  
--Name: Claes S  
--Date: 2008-12-08  
--Desc: DayOff - Rename Table, PreferenceRestriction, RotationRestriction, RotationDayRestriction - Rename column DayOff
---------------- 
EXEC sp_rename 'dbo.DayOff', DayOffTemplate
GO 
EXEC sp_rename 'dbo.PreferenceRestriction.DayOff', DayOffTemplate, 'COLUMN'
GO
EXEC sp_rename 'dbo.RotationRestriction.DayOff', DayOffTemplate, 'COLUMN'
GO
EXEC sp_rename 'dbo.RotationDayRestriction.DayOff', DayOffTemplate, 'COLUMN'
GO
----------------  
--Name: Claes S  
--Date: 2008-12-09  
--Desc: PersonDayOff - Rename DateTimeAncor, Add/Update Columns, Drop Column/FK DayOff
---------------- 
EXEC sp_rename 'dbo.PersonDayOff.DateTimeAnchor', Anchor, 'COLUMN'
GO
ALTER TABLE PersonDayOff ADD [TargetLength] [bigint] NULL
GO
ALTER TABLE PersonDayOff ADD [Flexibility] [bigint] NULL
GO
ALTER TABLE PersonDayOff ADD [Name] [nvarchar] (50) NULL
GO
ALTER TABLE PersonDayOff ADD [ShortName] [nvarchar](25) NULL
GO

UPDATE PersonDayOff
SET TargetLength = dt.TargetLength, Flexibility = dt.Flexibility, [Name] = dt.[Name], ShortName = dt.ShortName
FROM DayOffTemplate AS dt
WHERE PersonDayOff.DayOff = dt.id
GO

ALTER TABLE PersonDayOff 
ALTER COLUMN [TargetLength] [bigint] NOT NULL
GO
ALTER TABLE PersonDayOff 
ALTER COLUMN [Flexibility] [bigint] NOT NULL
GO
ALTER TABLE PersonDayOff 
ALTER COLUMN [Name] [nvarchar](50) NOT NULL
GO	

ALTER TABLE PersonDayOff 
DROP CONSTRAINT FK_PersonDayOff_DayOff
GO

ALTER TABLE PersonDayOff DROP COLUMN DayOff
GO

----------------  
--Name: David Jonsson
--Date: 2008-12-09  
--Desc: Adding needed table for running Freemimum
---------------- 
CREATE TABLE [dbo].[dim_queue](
	[queue_id] [int] IDENTITY(1,1) NOT NULL,
	[queue_agg_id] [int] NULL,
	[queue_code] [nvarchar](50) NULL,
	[queue_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_queue_queue_name]  DEFAULT ('Not Defined'),
	[log_object_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_dim_queue_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_dim_queue_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_queue_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_queue_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_dim_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[dim_interval](
	[interval_id] [smallint] NOT NULL,
	[interval_name] [nvarchar](20) NULL,
	[halfhour_name] [nvarchar](50) NULL,
	[hour_name] [nvarchar](50) NULL,
	[interval_start] [smalldatetime] NULL,
	[interval_end] [smalldatetime] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_interval] PRIMARY KEY CLUSTERED 
(
	[interval_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[dim_date](
	[date_id] [int] IDENTITY(1,1) NOT NULL,
	[date_date] [smalldatetime] NOT NULL,
	[year] [int] NOT NULL,
	[year_month] [int] NOT NULL,
	[month] [int] NOT NULL,
	[month_name] [nvarchar](20) NOT NULL,
	[month_resource_key] [nvarchar](100) NULL,
	[day_in_month] [int] NOT NULL,
	[weekday_number] [int] NOT NULL,
	[weekday_name] [nvarchar](20) NOT NULL,
	[weekday_resource_key] [nvarchar](100) NULL,
	[week_number] [int] NOT NULL,
	[year_week] [nvarchar](6) NOT NULL,
	[quarter] [nvarchar](6) NOT NULL,
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_date_inserted]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_date] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[fact_queue](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[queue_id] [int] NOT NULL,
	[local_date_id] [int] NULL,
	[local_interval_id] [smallint] NULL,
	[offered_calls] [decimal](24, 5) NULL,
	[answered_calls] [decimal](24, 5) NULL,
	[answered_calls_within_SL] [decimal](24, 5) NULL,
	[abandoned_calls] [decimal](24, 5) NULL,
	[abandoned_calls_within_SL] [decimal](24, 5) NULL,
	[abandoned_short_calls] [decimal](18, 0) NULL,
	[overflow_out_calls] [decimal](24, 5) NULL,
	[overflow_in_calls] [decimal](24, 5) NULL,
	[talk_time_s] [decimal](24, 5) NULL,
	[after_call_work_s] [decimal](24, 5) NULL,
	[handle_time_s] [decimal](24, 5) NULL,
	[speed_of_answer_s] [decimal](24, 5) NULL,
	[time_to_abandon_s] [decimal](24, 5) NULL,
	[longest_delay_in_queue_answered_s] [decimal](24, 5) NULL,
	[longest_delay_in_queue_abandoned_s] [decimal](24, 5) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_queue_statistics_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_queue_statistics_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_queue_statistics_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_queue_statistics_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_fact_queue] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[queue_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_date] FOREIGN KEY([date_id])
REFERENCES [dbo].[dim_date] ([date_id])
ALTER TABLE [dbo].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_date]

ALTER TABLE [dbo].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [dbo].[dim_interval] ([interval_id])
ALTER TABLE [dbo].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_interval]

ALTER TABLE [dbo].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [dbo].[dim_queue] ([queue_id])
ALTER TABLE [dbo].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_queue]
GO

----------------  
--Name: MadhurangaP
--Date: 2008-12-09  
--Desc: Manage values for ShiftTradeRequest & RequestPart DateTime columns according to domain changes
---------------- 

Update 
	ShiftTradeRequest 
Set 
	ShiftTradeRequest.RequestedStartDateTime = DATEADD(ss, -86399, RequestPart.EndDateTime) ,
    ShiftTradeRequest.RequestedEndDateTime = RequestPart.EndDateTime
FROM 
	RequestPart, ShiftTradeRequest
WHERE  
	RequestPart.Id = ShiftTradeRequest.RequestPart
GO

Update 
	RequestPart 
SET 
	EndDateTime =DATEADD(ss,86399, StartDateTime) 

GO

ALTER TABLE ShiftTradeRequest
ALTER COLUMN [RequestedStartDateTime] [datetime] NOT NULL
GO

ALTER TABLE ShiftTradeRequest
ALTER COLUMN [RequestedEndDateTime] [datetime] NOT NULL
GO

----------------  
--Name: Zoet
--Date: 2008-12-09  
--Desc: New tables - BudgetingRowDefinition, PlanningGroup
---------------- 
CREATE TABLE [dbo].[PlanningGroup](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[PlanningGroup]  WITH CHECK ADD  CONSTRAINT [FK_PlanningGroup_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PlanningGroup] CHECK CONSTRAINT [FK_PlanningGroup_BusinessUnit]
GO
ALTER TABLE [dbo].[PlanningGroup]  WITH CHECK ADD  CONSTRAINT [FK_PlanningGroup_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PlanningGroup] CHECK CONSTRAINT [FK_PlanningGroup_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PlanningGroup]  WITH CHECK ADD  CONSTRAINT [FK_PlanningGroup_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PlanningGroup] CHECK CONSTRAINT [FK_PlanningGroup_Person_UpdatedBy]
GO
CREATE TABLE [dbo].[BudgetingRowDefinition](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[ValueIsPositive] [bit] NOT NULL,
	[ValueIsPercentage] [bit] NOT NULL,
	[ShrinkageType] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[BudgetingRowDefinition]  WITH CHECK ADD  CONSTRAINT [FK_BudgetingRowDefinition_PlanningGroup] FOREIGN KEY([Parent])
REFERENCES [dbo].[PlanningGroup] ([Id])
GO
ALTER TABLE [dbo].[BudgetingRowDefinition] CHECK CONSTRAINT [FK_BudgetingRowDefinition_PlanningGroup]
GOIF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitStringString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
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
-- returnerar en tabell istället
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
-- returnerar en tabell istället
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
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (15,'7.0.15') 
