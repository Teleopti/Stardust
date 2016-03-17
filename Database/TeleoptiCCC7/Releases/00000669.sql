----------------  
--Name: Chundan Xu
--Date: 2016-03-17
--Desc: Adding read model load time table for ScheduleProjectionReadOnly read model.
----------------- 
CREATE TABLE [ReadModel].[PersonScheduleProjectionLoadTime](
		[PersonId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[ScheduleLoadedTime] [datetime] NOT NULL
	 CONSTRAINT [PK_PersonScheduleDay] PRIMARY KEY CLUSTERED 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)
	)