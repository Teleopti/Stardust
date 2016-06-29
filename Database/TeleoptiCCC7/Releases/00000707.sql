----------------  
--Name: Dumpling
--Date: 2016-06-29
--Desc: check result of validating ReadModel.ScheduleDay
----------------

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleDay_check]') AND type in (N'U'))
BEGIN
CREATE TABLE [ReadModel].[ScheduleDay_check]
	(
	PersonId uniqueidentifier NOT NULL,
	BelongsToDate datetime NOT NULL,
	IsValid bit NOT NULL,
	UpdatedOn datetime NOT NULL
	
	 CONSTRAINT [PK_ScheduleDay_check] PRIMARY KEY CLUSTERED 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)
	)  
END
GO
