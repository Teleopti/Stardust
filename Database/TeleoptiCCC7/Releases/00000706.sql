----------------  
--Name: Dumpling
--Date: 2016-06-29
--Desc: check result of validating ReadModel.PersonScheduleDay
----------------

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[PersonScheduleDay_check]') AND type in (N'U'))
BEGIN
CREATE TABLE [ReadModel].[PersonScheduleDay_check]
	(
	PersonId uniqueidentifier NOT NULL,
	BelongsToDate datetime NOT NULL,
	IsValid bit NOT NULL,
	UpdatedOn datetime NOT NULL
	
	 CONSTRAINT [PK_PersonScheduleDay_check] PRIMARY KEY CLUSTERED 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)
	)  
END
GO
