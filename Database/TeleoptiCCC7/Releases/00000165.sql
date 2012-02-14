/* 
Trunk initiated: 
2009-10-13 
08:27
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2009-10-13 
--Desc: Changed the domain for meetings
----------------  
CREATE TABLE [dbo].[NewMeeting](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[Organizer] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[Subject] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](2000) NOT NULL,
	[Location] [nvarchar](100) NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[StartTime] [bigint] NOT NULL,
	[EndTime] [bigint] NOT NULL,
	[TimeZone] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

GO
CREATE TABLE [dbo].[RecurrentDailyMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[NewRecurrentMeetingOption](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[IncrementCount] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[RecurrentMonthlyByDayMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
	[DayInMonth] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[RecurrentMonthlyByWeekMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[WeekOfMonth] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[RecurrentWeeklyMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[RecurrentWeeklyMeetingWeekDays](
	[RecurrentWeeklyMeeting] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NULL
) ON [PRIMARY]

GO

DECLARE @hoursDifference int
SET @hoursDifference = DateDiff(hour, getutcdate(),getdate())

--Start with meetings without recurring information
INSERT INTO NewMeeting (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Organizer, Scenario, [Subject], [Description], Location, Activity, TimeZone, StartDate, EndDate, BusinessUnit, StartTime, EndTime)
SELECT Meeting.Id, Meeting.Version, Meeting.CreatedBy, Meeting.UpdatedBy, Meeting.CreatedOn, Meeting.UpdatedOn, Meeting.CreatedBy, Meeting.Scenario, Meeting.[Subject], Meeting.[Description], Meeting.Location, Meeting.PayLoad, IsNull(Person.DefaultTimeZone,'Utc'), DateAdd(hour,@hoursDifference,Meeting.Minimum), DateAdd(hour,@hoursDifference,Meeting.Minimum), Meeting.BusinessUnit, CAST((DatePart(hour, DateAdd(hour,@hoursDifference,Meeting.Minimum)) * 60) + DatePart(minute, DateAdd(hour,@hoursDifference,Meeting.Minimum)) as bigint) * 60 * 10000000, CAST((DatePart(hour, DateAdd(hour,@hoursDifference,Meeting.Maximum)) * 60) + DatePart(minute, DateAdd(hour,@hoursDifference,Meeting.Maximum)) as bigint) * 60 * 10000000 FROM Meeting INNER JOIN Person ON Person.Id=Meeting.CreatedBy WHERE IsRecurrent=0

INSERT INTO NewRecurrentMeetingOption (Id, Parent, IncrementCount)
SELECT NEWID(), Id, 1 FROM NewMeeting

INSERT INTO RecurrentDailyMeeting (RecurrentMeetingOption)
SELECT Id FROM NewRecurrentMeetingOption

--Now let's do the weekly recurring meetings
INSERT INTO NewMeeting (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Organizer, Scenario, [Subject], [Description], Location, Activity, TimeZone, StartDate, EndDate, BusinessUnit, StartTime, EndTime)
SELECT Meeting.Id, Meeting.Version, Meeting.CreatedBy, Meeting.UpdatedBy, Meeting.CreatedOn, Meeting.UpdatedOn, Meeting.CreatedBy, Meeting.Scenario, Meeting.[Subject], Meeting.[Description], Meeting.Location, Meeting.PayLoad, IsNull(Person.DefaultTimeZone,'Utc'), DateAdd(hour,@hoursDifference,Meeting.Minimum), DateAdd(hour,@hoursDifference,Meeting.Maximum), Meeting.BusinessUnit, Meeting.StartTime, Meeting.EndTime FROM Meeting INNER JOIN Person ON Person.Id=Meeting.CreatedBy WHERE IsRecurrent=1 AND RecurrentMeetingType=1

INSERT INTO NewRecurrentMeetingOption (Id, Parent, IncrementCount)
SELECT NEWID(), Id, RecurOn FROM Meeting WHERE IsRecurrent=1 AND RecurrentMeetingType=1

INSERT INTO RecurrentWeeklyMeeting (RecurrentMeetingOption)
SELECT NewRecurrentMeetingOption.Id FROM NewRecurrentMeetingOption INNER JOIN Meeting ON Meeting.Id=NewRecurrentMeetingOption.Parent WHERE IsRecurrent=1 AND RecurrentMeetingType=1

INSERT INTO RecurrentWeeklyMeetingWeekDays (RecurrentWeeklyMeeting, [DayOfWeek])
SELECT RecurrentWeeklyMeeting.RecurrentMeetingOption,RecurrentMeetingOption.Day FROM RecurrentWeeklyMeeting INNER JOIN NewRecurrentMeetingOption ON NewRecurrentMeetingOption.Id=RecurrentWeeklyMeeting.RecurrentMeetingOption INNER JOIN Meeting ON Meeting.Id=NewRecurrentMeetingOption.Parent INNER JOIN RecurrentMeetingOption ON RecurrentMeetingOption.Parent=Meeting.Id WHERE IsRecurrent=1 AND RecurrentMeetingType=1

--Ok let's finish with the monthly recurring meetings
INSERT INTO NewMeeting (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Organizer, Scenario, [Subject], [Description], Location, Activity, TimeZone, StartDate, EndDate, BusinessUnit, StartTime, EndTime)
SELECT Meeting.Id, Meeting.Version, Meeting.CreatedBy, Meeting.UpdatedBy, Meeting.CreatedOn, Meeting.UpdatedOn, Meeting.CreatedBy, Meeting.Scenario, Meeting.[Subject], Meeting.[Description], Meeting.Location, Meeting.PayLoad, IsNull(Person.DefaultTimeZone,'Utc'), DateAdd(hour,@hoursDifference,Meeting.Minimum), DateAdd(hour,@hoursDifference,Meeting.Maximum), Meeting.BusinessUnit, Meeting.StartTime, Meeting.EndTime FROM Meeting INNER JOIN Person ON Person.Id=Meeting.CreatedBy WHERE IsRecurrent=1 AND RecurrentMeetingType=2

INSERT INTO NewRecurrentMeetingOption (Id, Parent, IncrementCount)
SELECT NEWID(), Id, RecurOn FROM Meeting WHERE IsRecurrent=1 AND RecurrentMeetingType=2

INSERT INTO RecurrentMonthlyByWeekMeeting (RecurrentMeetingOption, DayOfWeek, WeekOfMonth)
SELECT NewRecurrentMeetingOption.Id, RecurrentMeetingOption.Day, RecurrentMeetingOption.Week - 1 FROM NewRecurrentMeetingOption INNER JOIN Meeting ON Meeting.Id=NewRecurrentMeetingOption.Parent INNER JOIN RecurrentMeetingOption ON RecurrentMeetingOption.Parent=Meeting.Id WHERE IsRecurrent=1 AND RecurrentMeetingType=2
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Meeting_Activity]') AND parent_object_id = OBJECT_ID(N'[dbo].[Meeting]'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT [FK_Meeting_Activity]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Meeting_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[Meeting]'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT [FK_Meeting_BusinessUnit]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Meeting_Person_CreatedBy]') AND parent_object_id = OBJECT_ID(N'[dbo].[Meeting]'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT [FK_Meeting_Person_CreatedBy]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Meeting_Person_UpdatedBy]') AND parent_object_id = OBJECT_ID(N'[dbo].[Meeting]'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT [FK_Meeting_Person_UpdatedBy]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Meeting_Person3]') AND parent_object_id = OBJECT_ID(N'[dbo].[Meeting]'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT [FK_Meeting_Person3]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Meeting_Scenario]') AND parent_object_id = OBJECT_ID(N'[dbo].[Meeting]'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT [FK_Meeting_Scenario]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RecurrentMeeting_Meeting]') AND parent_object_id = OBJECT_ID(N'[dbo].[RecurrentMeeting]'))
ALTER TABLE [dbo].[RecurrentMeeting] DROP CONSTRAINT [FK_RecurrentMeeting_Meeting]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RecurrentMeetingOption_Meeting]') AND parent_object_id = OBJECT_ID(N'[dbo].[RecurrentMeetingOption]'))
ALTER TABLE [dbo].[RecurrentMeetingOption] DROP CONSTRAINT [FK_RecurrentMeetingOption_Meeting]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MeetingPerson_Meeting]') AND parent_object_id = OBJECT_ID(N'[dbo].[MeetingPerson]'))
ALTER TABLE [dbo].[MeetingPerson] DROP CONSTRAINT [FK_MeetingPerson_Meeting]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Meeting]') AND type in (N'U'))
DROP TABLE [dbo].[Meeting]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurrentMeeting]') AND type in (N'U'))
DROP TABLE [dbo].[RecurrentMeeting]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurrentMeetingOption]') AND type in (N'U'))
DROP TABLE [dbo].[RecurrentMeetingOption]
GO

sp_rename 'dbo.NewMeeting','Meeting','OBJECT'
GO

sp_rename 'dbo.NewRecurrentMeetingOption','RecurrentMeetingOption','OBJECT'
GO

ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Activity]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_BusinessUnit]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Person3] FOREIGN KEY([Organizer])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Person3]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Scenario]
GO
ALTER TABLE [dbo].[RecurrentDailyMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentDailyMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentDailyMeeting] CHECK CONSTRAINT [FK_RecurrentDailyMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentMeetingOption]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentMeetingOption_Meeting] FOREIGN KEY([Parent])
REFERENCES [dbo].[Meeting] ([Id])
GO
ALTER TABLE [dbo].[RecurrentMeetingOption] CHECK CONSTRAINT [FK_RecurrentMeetingOption_Meeting]
GO
ALTER TABLE [dbo].[RecurrentMonthlyByDayMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentMonthlyByDayMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentMonthlyByDayMeeting] CHECK CONSTRAINT [FK_RecurrentMonthlyByDayMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentMonthlyByWeekMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentMonthlyByWeekMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentMonthlyByWeekMeeting] CHECK CONSTRAINT [FK_RecurrentMonthlyByWeekMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentWeeklyMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeeting] CHECK CONSTRAINT [FK_RecurrentWeeklyMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeetingWeekDays]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting] FOREIGN KEY([RecurrentWeeklyMeeting])
REFERENCES [dbo].[RecurrentWeeklyMeeting] ([RecurrentMeetingOption])
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeetingWeekDays] CHECK CONSTRAINT [FK_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting]
GO
ALTER TABLE [dbo].[MeetingPerson]  WITH CHECK ADD  CONSTRAINT [FK_MeetingPerson_Meeting] FOREIGN KEY([Parent])
REFERENCES [dbo].[Meeting] ([Id])
GO
ALTER TABLE [dbo].[MeetingPerson] CHECK CONSTRAINT [FK_MeetingPerson_Meeting]
GO

----------------  
--Name: Ola HÂkansson
--Date: 2009-10-19 
--Desc: Changes to Preferences
----------------

/****** Object:  Table [dbo].[PreferenceRestrictionNew]    Script Date: 10/13/2009 14:19:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PreferenceRestrictionNew](
	[RestrictionBase] [uniqueidentifier] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
	[DayOffTemplate] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[RestrictionBase] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PreferenceRestrictionNew]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRest_DayOff] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestrictionNew] CHECK CONSTRAINT [FK_PreferenceRest_DayOff]
GO

ALTER TABLE [dbo].[PreferenceRestrictionNew]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRest_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestrictionNew] CHECK CONSTRAINT [FK_PreferenceRest_ShiftCategory]
GO

ALTER TABLE [dbo].[PreferenceRestrictionNew]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestriction_RestrictionBase] FOREIGN KEY([RestrictionBase])
REFERENCES [dbo].[RestrictionBase] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestrictionNew] CHECK CONSTRAINT [FK_PreferenceRestriction_RestrictionBase]
GO

/****** Object:  Table [dbo].[ActivityRestriction]    Script Date: 10/13/2009 14:21:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ActivityRestriction](
	[RestrictionBase] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
	[Activity] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[RestrictionBase] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ActivityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestriction_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO

ALTER TABLE [dbo].[ActivityRestriction] CHECK CONSTRAINT [FK_ActivityRestriction_Activity]
GO

ALTER TABLE [dbo].[ActivityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestriction_PreferenceRestriction] FOREIGN KEY([Parent])
REFERENCES [dbo].[PreferenceRestrictionNew] ([RestrictionBase])
GO

ALTER TABLE [dbo].[ActivityRestriction] CHECK CONSTRAINT [FK_ActivityRestriction_PreferenceRestriction]
GO

ALTER TABLE [dbo].[ActivityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestriction_RestrictionBase] FOREIGN KEY([RestrictionBase])
REFERENCES [dbo].[RestrictionBase] ([Id])
GO

ALTER TABLE [dbo].[ActivityRestriction] CHECK CONSTRAINT [FK_ActivityRestriction_RestrictionBase]
GO

/****** Object:  Table [dbo].[PreferenceDay]    Script Date: 10/13/2009 14:19:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PreferenceDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[RestrictionDate] [datetime] NOT NULL,
	[Restriction] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_BusinessUnit]
GO

ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person]
GO

ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person_CreatedBy]
GO

ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestrictionNew_Restriction] FOREIGN KEY([Restriction])
REFERENCES [dbo].[PreferenceRestrictionNew] ([RestrictionBase])
GO

ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceRestrictionNew_Restriction]
GO


-- insert old preferences
Insert into RestrictionBase
SELECT Id,StartTimeMinimum, StartTimeMaximum,EndTimeMinimum, EndTimeMaximum,
WorkTimeMinimum, WorkTimeMaximum
FROM ScheduleRestriction 
where Id in(select ScheduleRestriction FROM PreferenceRestriction)

INSERT INTO PreferenceRestrictionNew
SELECT PreferenceRestriction.ScheduleRestriction, PreferenceRestriction.ShiftCategory,
PreferenceRestriction.DayOffTemplate
FROM PreferenceRestriction

DECLARE @ScheduleRestriction uniqueidentifier
DECLARE @Activity uniqueidentifier
DECLARE @newID uniqueidentifier
DECLARE act_cursor CURSOR FOR  
SELECT ScheduleRestriction, Activity 
FROM PreferenceRestriction WHERE Activity IS Not Null

OPEN act_cursor   
FETCH NEXT FROM act_cursor INTO @ScheduleRestriction, @Activity   

WHILE @@FETCH_STATUS = 0   
BEGIN   
      SELECT @newID = NEWID()
      INSERT INTO RestrictionBase
      SELECT @newID, 0,0,0,0,0,0
      

      INSERT INTO ActivityRestriction
      SELECT @newID, @ScheduleRestriction,  @Activity
      
      FETCH NEXT FROM act_cursor INTO @ScheduleRestriction, @Activity 
END   

CLOSE act_cursor   
DEALLOCATE act_cursor 

-- the day
INSERT [PreferenceDay]
SELECT PersonRestriction.Id, Version, CreatedBy, isnull(UpdatedBy,CreatedBy), CreatedOn, isnull(UpdatedOn,CreatedOn), Person, RestrictionDate, 
ScheduleRestriction.Id, BusinessUnit FROM PersonRestriction
Inner JOIN ScheduleRestriction
ON ScheduleRestriction.Parent = PersonRestriction.Id 
Inner join PreferenceRestriction ON ScheduleRestriction.Id = PreferenceRestriction.ScheduleRestriction
AND IsDeleted = 0
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
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (165,'7.0.165') 
