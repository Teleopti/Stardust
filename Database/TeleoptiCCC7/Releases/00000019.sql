/* 
BuildTime is: 
2009-01-12 
12:11
*/ 
/* 
Trunk initiated: 
2008-12-30 
11:40
By: TOPTINET\NiclasH 
On TELEOPTI566 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2008-xx-xx  
--Desc: Because ...  
----------------  

----------------  
--Name: T.A.D.V.D. Siriwardana
--Date: 2009-01-07
--Desc: Recurrent meeting table
----------------  

ALTER TABLE [Meeting] ADD [StartTime]	[bigint] NULL 
ALTER TABLE [Meeting] ADD [EndTime]		[bigint] NULL 
GO

UPDATE [Meeting] SET [StartTime] = 0, [EndTime] = 0

ALTER TABLE [Meeting] ALTER COLUMN [StartTime]	[bigint] NOT NULL 
ALTER TABLE [Meeting] ALTER COLUMN [EndTime]	[bigint] NOT NULL 


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecurrentMeeting]
(
	[Id]			[uniqueidentifier]	NOT NULL,
	[RecurringDate] [datetime]			NOT NULL,
	[StartTime]		[bigint]			NOT NULL,
	[EndTime]		[bigint]			NOT NULL,
	[Parent]		[uniqueidentifier]	NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
	ON [PRIMARY]
) 
ON [PRIMARY]

GO
ALTER TABLE [dbo].[RecurrentMeeting] WITH CHECK ADD CONSTRAINT [FK_RecurrentMeeting_Meeting] FOREIGN KEY([Parent]) REFERENCES [dbo].[Meeting] ([Id])
GO
ALTER TABLE [dbo].[RecurrentMeeting] CHECK CONSTRAINT [FK_RecurrentMeeting_Meeting]
GO

----------------  
--Name: MadhurangaP
--Date: 2009-01-11
--Desc: Multiplicator table
----------------  
CREATE TABLE [dbo].[Multiplicator](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[MultiplicatorType] [int] NOT NULL,
	[MultiplicatorValue] [float] NOT NULL,
	[ExportCode] [nvarchar](255) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Multiplicator]  WITH CHECK ADD  CONSTRAINT [FK_Multiplicator_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Multiplicator] CHECK CONSTRAINT [FK_Multiplicator_BusinessUnit]
GO
ALTER TABLE [dbo].[Multiplicator]  WITH CHECK ADD  CONSTRAINT [FK_Multiplicator_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Multiplicator] CHECK CONSTRAINT [FK_Multiplicator_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Multiplicator]  WITH CHECK ADD  CONSTRAINT [FK_Multiplicator_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Multiplicator] CHECK CONSTRAINT [FK_Multiplicator_Person_UpdatedBy]IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitStringString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
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
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (19,'7.0.19') 
