
/****** Object:  Index [CIX_ScheduleProjectionReadOnly]    Script Date: 2016-08-16 14:10:28 ******/
DROP INDEX [CIX_ScheduleProjectionReadOnly] ON [ReadModel].[ScheduleProjectionReadOnly] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [CIX_ScheduleProjectionReadOnly]    Script Date: 2016-08-16 14:10:28 ******/
CREATE CLUSTERED INDEX [CIX_ScheduleProjectionReadOnly] ON [ReadModel].[ScheduleProjectionReadOnly]
(
	[PersonId] ASC,
	[BelongsToDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
