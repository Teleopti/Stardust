----------------  
--Name: RogerKr
--Date: 2011-09-23
--Desc: Adds tables for new auditing (envers)
----------------  

CREATE SCHEMA [Auditing] AUTHORIZATION [dbo]
GO

--REVISION TABLE
CREATE TABLE [Auditing].[Revision](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ModifiedAt] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NOT NULL)
ALTER TABLE [Auditing].[Revision] ADD CONSTRAINT [PK_Revision]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC
)


ALTER TABLE [Auditing].[Revision]  WITH CHECK ADD  CONSTRAINT [FK_Revision_Person] FOREIGN KEY([ModifiedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [Auditing].[Revision] CHECK CONSTRAINT [FK_Revision_Person]


---PERSON DAYOFF
CREATE TABLE [Auditing].[PersonDayOff_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Version] [int] NULL,
	[Anchor] [datetime] NULL,
	[TargetLength] [bigint] NULL,
	[Flexibility] [bigint] NULL,
	[DisplayColor] [int] NULL,
	[PayrollCode] [nvarchar](20) NULL,
	[Name] [nvarchar](50) NULL,
	[ShortName] [nvarchar](25) NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[PersonDayOff_AUD] ADD CONSTRAINT [PK_persondayoff_aud]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
	

ALTER TABLE [Auditing].[PersonDayOff_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[PersonDayOff_AUD] CHECK CONSTRAINT [FK_PersonDayOff_REV]


--PERSON ASSIGNMENT
CREATE TABLE [Auditing].[PersonAssignment_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Version] [int] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[PersonAssignment_AUD] ADD CONSTRAINT [PK_personassignment_aud]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[PersonAssignment_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[PersonAssignment_AUD] CHECK CONSTRAINT [FK_PersonAssignment_REV]

--PERSONALSHIFT ACTIVITYLAYER
CREATE TABLE [Auditing].[PersonalShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD] ADD CONSTRAINT [PK_personalshiftactivitylayer_aud]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayer_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_PersonalShiftActivityLayer_REV]


---PERSONALSHIFT
CREATE TABLE [Auditing].[PersonalShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[PersonalShift_AUD] ADD CONSTRAINT [PK_personalshift_aud]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[PersonalShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShift_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[PersonalShift_AUD] CHECK CONSTRAINT [FK_PersonalShift_REV]


--PERSONAL ABSENCE
CREATE TABLE [Auditing].[PersonAbsence_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Version] [int] NULL,
	[LastChange] [datetime] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[Payload] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[PersonAbsence_AUD] ADD CONSTRAINT [PK_personAbsence_aud]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[PersonAbsence_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalAbsence_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[PersonAbsence_AUD] CHECK CONSTRAINT [FK_PersonalAbsence_REV]


--Overtimeshift activity layer
CREATE TABLE [Auditing].[OvertimeShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[DefinitionSet] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD] ADD CONSTRAINT [PK_OvertimeShiftActivityLayer_AUD]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_REV]


--Overtime shift
CREATE TABLE [Auditing].[OvertimeShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[OvertimeShift_AUD] ADD CONSTRAINT [PK_OvertimeShift_AUD]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[OvertimeShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShift_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[OvertimeShift_AUD] CHECK CONSTRAINT [FK_OvertimeShift_REV]


--MainShift activity layer
CREATE TABLE [Auditing].[MainShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD] ADD CONSTRAINT [PK_MainShiftActivityLayer_AUD]	
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_MainShiftActivityLayer_REV]


--MainShift
CREATE TABLE [Auditing].[MainShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[RefId] [uniqueidentifier] NULL,
	[ShiftCategory] [uniqueidentifier] NULL)
ALTER TABLE [Auditing].[MainShift_AUD] ADD CONSTRAINT [PK_MainShift_AUD]	
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC,
			  [REV] ASC
)	
ALTER TABLE [Auditing].[MainShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShift_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE
ALTER TABLE [Auditing].[MainShift_AUD] CHECK CONSTRAINT [FK_MainShift_REV]


CREATE TABLE [Auditing].[AuditSetting](
	[Id] [int] NOT NULL,
	[IsScheduleEnabled] [bit] NOT NULL)
ALTER TABLE [Auditing].[AuditSetting] ADD CONSTRAINT [PK_AuditSetting]
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)

GO

----------------  
--Name: RogerKr
--Date: 2011-09-27
--Desc: Dropping old envers tables
----------------  
drop table dbo.personassignment_aud
drop table dbo.mainshift_aud
drop table dbo.mainshiftactivitylayer_aud
drop table dbo.personalshift_aud
drop table dbo.personalshiftactivitylayer_aud
drop table dbo.overtimeshift_aud
drop table dbo.overtimeshiftactivitylayer_aud
drop table dbo.revinfo

----------------  
--Name: RogerKr
--Date: 2011-09-27
--Desc: Adding auditsetting row
----------------  
insert into [Auditing].[AuditSetting] (Id,IsScheduleEnabled)
values (1, 0)

----------------  
--Name: TamasB
--Date: 2011-09-26
--Desc: Add Seasonability column to SchedulePeriod table 
----------------  
ALTER TABLE dbo.SchedulePeriod ADD
	Seasonality float(53) NOT NULL CONSTRAINT DF_SchedulePeriod_Seasonality DEFAULT 0
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (336,'7.1.336') 
