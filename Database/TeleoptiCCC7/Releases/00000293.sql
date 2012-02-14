/* 
Trunk initiated: 
2010-09-03 
13:16
By: TOPTINET\johanr 
On TELEOPTI565 
*/  
----------------  
--Name: Robin Karlsson
--Date: 2010-08-25
--Desc: Extended preferences template
----------------  
CREATE TABLE [dbo].[ActivityRestrictionTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[Activity] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[ExtendedPreferenceTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DisplayColor] [int] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[PreferenceRestrictionTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[MustHave] [bit] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
	[DayOffTemplate] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ActivityRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestrictionTemplate_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO

ALTER TABLE [dbo].[ActivityRestrictionTemplate] CHECK CONSTRAINT [FK_ActivityRestrictionTemplate_Activity]
GO

ALTER TABLE [dbo].[ActivityRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestrictionTemplate_PreferenceRestrictionTemplate] FOREIGN KEY([Id])
REFERENCES [dbo].[PreferenceRestrictionTemplate] ([Id])
GO

ALTER TABLE [dbo].[ActivityRestrictionTemplate] CHECK CONSTRAINT [FK_ActivityRestrictionTemplate_PreferenceRestrictionTemplate]
GO

ALTER TABLE [dbo].[ActivityRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FKDF16F7969265A386] FOREIGN KEY([Parent])
REFERENCES [dbo].[PreferenceRestrictionTemplate] ([Id])
GO

ALTER TABLE [dbo].[ActivityRestrictionTemplate] CHECK CONSTRAINT [FKDF16F7969265A386]
GO

ALTER TABLE [dbo].[ExtendedPreferenceTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ExtendedPreferenceTemplate_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExtendedPreferenceTemplate] CHECK CONSTRAINT [FK_ExtendedPreferenceTemplate_Person]
GO

ALTER TABLE [dbo].[ExtendedPreferenceTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExtendedPreferenceTemplate] CHECK CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_CreatedBy]
GO

ALTER TABLE [dbo].[ExtendedPreferenceTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExtendedPreferenceTemplate] CHECK CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestictionTemplate_DayOff] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate] CHECK CONSTRAINT [FK_PreferenceRestictionTemplate_DayOff]
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestrictionTemplate_ExtendedPreferenceTemplate] FOREIGN KEY([Id])
REFERENCES [dbo].[ExtendedPreferenceTemplate] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate] CHECK CONSTRAINT [FK_PreferenceRestrictionTemplate_ExtendedPreferenceTemplate]
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestrictionTemplate_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate] CHECK CONSTRAINT [FK_PreferenceRestrictionTemplate_ShiftCategory]
GO

----------------  
--Name: Robin Karlsson
--Date: 2010-08-30
--Desc: Extended preferences template name for preference days
----------------  
ALTER TABLE [dbo].[PreferenceDay] ADD [TemplateName] [nvarchar](50) NULL
GO
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (293,'7.1.293') 
