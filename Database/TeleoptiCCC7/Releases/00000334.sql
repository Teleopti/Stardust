/* 
Trunk initiated: 
2011-08-31 
08:22
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

----------------  
-- Name: RobinK
-- Date: 2011-08-30  
-- Desc: Add new tables for job result
-----------------
CREATE TABLE [dbo].[JobResult](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[JobCategory] [nvarchar](50) NOT NULL,
	[FinishedOk] [bit] NOT NULL,
	[Owner] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL
	)
	
ALTER TABLE dbo.JobResult ADD CONSTRAINT
	PK_JobResult PRIMARY KEY CLUSTERED 
	(
	Id
	)
	
CREATE TABLE [dbo].[JobResultDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Message] [nvarchar](1000) NOT NULL,
	[ExceptionStackTrace] [nvarchar](max) NULL,
	[ExceptionMessage] [nvarchar](2000) NULL,
	[InnerExceptionStackTrace] [nvarchar](max) NULL,
	[InnerExceptionMessage] [nvarchar](2000) NULL,
	[Timestamp] [datetime] NOT NULL,
	[DetailLevel] [int] NOT NULL
	)

ALTER TABLE dbo.JobResultDetail ADD CONSTRAINT
	PK_JobResultDetail PRIMARY KEY NONCLUSTERED 
	(
	Id
	)
	
CREATE CLUSTERED INDEX [CIX_JobResultDetail_Parent] ON [dbo].[JobResultDetail] 
(
	[Parent] ASC
)


ALTER TABLE [dbo].[JobResult]  WITH CHECK ADD  CONSTRAINT [FK_JobResult_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[JobResult] CHECK CONSTRAINT [FK_JobResult_BusinessUnit]
GO

ALTER TABLE [dbo].[JobResult]  WITH CHECK ADD  CONSTRAINT [FK_JobResult_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[JobResult] CHECK CONSTRAINT [FK_JobResult_Person_CreatedBy]
GO

ALTER TABLE [dbo].[JobResult]  WITH CHECK ADD  CONSTRAINT [FK_JobResult_Person_Owner] FOREIGN KEY([Owner])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[JobResult] CHECK CONSTRAINT [FK_JobResult_Person_Owner]
GO

ALTER TABLE [dbo].[JobResult]  WITH CHECK ADD  CONSTRAINT [FK_JobResult_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[JobResult] CHECK CONSTRAINT [FK_JobResult_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[JobResultDetail]  WITH CHECK ADD  CONSTRAINT [FK_JobResultDetail_JobResult] FOREIGN KEY([Parent])
REFERENCES [dbo].[JobResult] ([Id])
GO

ALTER TABLE [dbo].[JobResultDetail] CHECK CONSTRAINT [FK_JobResultDetail_JobResult]
GO

/*
   den 24 augusti
   User: tamasb
   Database: new table
*/
CREATE TABLE [dbo].[WorkflowControlSetAllowedAbsences](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[Absence] [uniqueidentifier] NOT NULL
)

GO

CREATE CLUSTERED INDEX [CIX_WorkflowControlSetAllowedAbsences_WorkflowControlSet]
ON [dbo].[WorkflowControlSetAllowedAbsences] 
(
	[WorkflowControlSet] ASC
)
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsences]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedAbsences_Absence] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsences] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedAbsences_Absence]
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsences]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedAbsences_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsences] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedAbsences_WorkflowControlSet]
GO

/*
   den 29 augusti
   User: CS
   Database: new column in PreferenceRestriction, Absence
*/
ALTER TABLE dbo.PreferenceRestriction ADD Absence uniqueidentifier NULL
GO

ALTER TABLE [dbo].[PreferenceRestriction]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRest_Absence] FOREIGN KEY([Absence]) REFERENCES [dbo].[Absence] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestriction] CHECK CONSTRAINT [FK_PreferenceRest_Absence]
GO

/*
   den 6 september
   User: CS
   Database: new column in PreferenceRestrictionTemplate, Absence
*/
ALTER TABLE dbo.PreferenceRestrictionTemplate ADD Absence uniqueidentifier NULL
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestriction_Absence] FOREIGN KEY([Absence]) REFERENCES [dbo].[Absence] ([Id])
GO

ALTER TABLE [dbo].[PreferenceRestrictionTemplate] CHECK CONSTRAINT [FK_PreferenceRestriction_Absence]
GO


	

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (334,'7.1.334') 
