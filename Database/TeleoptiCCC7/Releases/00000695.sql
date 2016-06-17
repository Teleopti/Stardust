----------------  
--Name: Dumpling
--Date: 2016-06-17  
--Desc: check result of validating ReadModel.ScheduleProjectionReadOnly
----------------  
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'CheckReadModel')
EXEC sp_executesql N'CREATE SCHEMA [CheckReadModel] AUTHORIZATION [dbo]'
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CheckReadModel].[ScheduleProjectionReadOnly]') AND type in (N'U'))
BEGIN
CREATE TABLE [CheckReadModel].[ScheduleProjectionReadOnly]
	(
	PersonId uniqueidentifier NOT NULL,
	BelongsToDate datetime NOT NULL,
	IsValid bit NOT NULL,
	UpdateOn datetime NOT NULL
	)  
	ON [PRIMARY]
ALTER TABLE [CheckReadModel].[ScheduleProjectionReadOnly] ADD CONSTRAINT PK_PersonId_BelongsToDate PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC,
	[BelongsToDate] ASC
)
ON [PRIMARY]
END
GO
