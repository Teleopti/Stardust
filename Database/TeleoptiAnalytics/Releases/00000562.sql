--Create new temp tables
CREATE TABLE [HangFire].[Job2](
	[Id] [int] NOT NULL,
	[StateId] [int] NULL,
	[StateName] [nvarchar](20) NULL,
	[InvocationData] [nvarchar](max) NOT NULL,
	[Arguments] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Job2] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_HangFire_Job2_StateName] ON [HangFire].[Job2]
(
	[StateName] ASC
)
GO

CREATE TABLE [HangFire].[JobParameter2](
	[Id] [int] NOT NULL,
	[JobId] [int] NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_JobParameter2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_HangFire_JobParameter2_JobIdAndName] ON [HangFire].[JobParameter2]
(
	[JobId] ASC,
	[Name] ASC
)
GO
CREATE TABLE [HangFire].[State2](
	[Id] [int] NOT NULL,
	[JobId] [int] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Reason] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Data] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_State2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_HangFire_State2_JobId] ON [HangFire].[State2]
(
	[JobId] ASC
)
GO

--Insert active into new tables
INSERT INTO [HangFire].[Job2]
           (Id,
			[StateId]
           ,[StateName]
           ,[InvocationData]
           ,[Arguments]
           ,[CreatedAt]
           ,[ExpireAt])
SELECT		Id,
			[StateId]
           ,[StateName]
           ,[InvocationData]
           ,[Arguments]
           ,[CreatedAt]
           ,[ExpireAt]
FROM Hangfire.Job
WHERE StateName not in ('Failed','Succeeded')
GO

INSERT INTO [HangFire].[JobParameter2]
SELECT		jp.Id,
			jp.[JobId]
           ,jp.[Name]
           ,jp.[Value]
FROM Hangfire.JobParameter jp
inner join Hangfire.Job2 j2 on j2.Id = jp.JobId
GO

INSERT INTO [HangFire].[State2]
SELECT		s.Id,
			s.[JobId]
           ,s.[Name]
           ,s.[Reason]
           ,s.[CreatedAt]
           ,s.[Data]
FROM Hangfire.State s
inner join Hangfire.Job2 j2 on j2.Id = s.JobId
GO

--Remove identity? no lets not bother.

--Drop FKs
ALTER TABLE [HangFire].[State] DROP CONSTRAINT [FK_HangFire_State_Job]
GO
ALTER TABLE [HangFire].[JobParameter] DROP CONSTRAINT [FK_HangFire_JobParameter_Job]
GO

--Truncate tables
truncate table Hangfire.JobParameter
truncate table Hangfire.State
truncate table Hangfire.Job
GO

SET IDENTITY_INSERT [HangFire].[Job] ON
--insert back
INSERT INTO [HangFire].[Job]
           ([Id]
		   ,[StateId]
           ,[StateName]
           ,[InvocationData]
           ,[Arguments]
           ,[CreatedAt]
           ,[ExpireAt])
SELECT		[Id]
			,[StateId]
           ,[StateName]
           ,[InvocationData]
           ,[Arguments]
           ,[CreatedAt]
           ,[ExpireAt]
FROM Hangfire.Job2
SET IDENTITY_INSERT [HangFire].[Job] OFF
GO

INSERT INTO [HangFire].[JobParameter]
           ([JobId]
           ,[Name]
           ,[Value])
SELECT		[JobId]
           ,[Name]
           ,[Value]
FROM Hangfire.JobParameter2
GO

INSERT INTO [HangFire].[State]
           ([JobId]
           ,[Name]
           ,[Reason]
           ,[CreatedAt]
           ,[Data])
SELECT		[JobId]
           ,[Name]
           ,[Reason]
           ,[CreatedAt]
           ,[Data]
FROM Hangfire.State2
GO

--Add FKs
ALTER TABLE [HangFire].[State]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_State_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [HangFire].[State] CHECK CONSTRAINT [FK_HangFire_State_Job]
GO

ALTER TABLE [HangFire].[JobParameter]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_JobParameter_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [HangFire].[JobParameter] CHECK CONSTRAINT [FK_HangFire_JobParameter_Job]
GO

--Drop temp tables
drop table hangfire.Job2
drop table hangfire.JobParameter2
drop table hangfire.State2
GO