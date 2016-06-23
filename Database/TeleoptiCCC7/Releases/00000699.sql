----------------  
--Name: Dumpling
--Date: 2016-06-23  
--Desc: check result of validating ReadModel.ScheduleProjectionReadOnly
----------------  
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CheckReadModel].[ScheduleProjectionReadOnly]') AND type in (N'U'))
DROP TABLE [CheckReadModel].[ScheduleProjectionReadOnly]

IF EXISTS (SELECT * FROM sys.schemas WHERE name = N'CheckReadModel')
EXEC sp_executesql N'DROP SCHEMA [CheckReadModel]'
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleProjectionReadOnly_check]') AND type in (N'U'))
BEGIN
CREATE TABLE [ReadModel].[ScheduleProjectionReadOnly_check]
	(
	PersonId uniqueidentifier NOT NULL,
	BelongsToDate datetime NOT NULL,
	IsValid bit NOT NULL,
	UpdateOn datetime NOT NULL
	
	 CONSTRAINT [PK_ScheduleProjectionReadOnly_check] PRIMARY KEY CLUSTERED 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)
	)  
END
GO