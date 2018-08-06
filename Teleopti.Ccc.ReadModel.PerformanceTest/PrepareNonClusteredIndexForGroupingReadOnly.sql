IF EXISTS(SELECT * FROM sysindexes WHERE name='IX_GroupingReadOnly_BusinessUnitId_LeavingDate_StartDate_EndDate')
	DROP INDEX [IX_GroupingReadOnly_BusinessUnitId_LeavingDate_StartDate_EndDate] ON [ReadModel].[GroupingReadOnly]

CREATE NONCLUSTERED INDEX [IX_GroupingReadOnly_BusinessUnitId_LeavingDate_StartDate_EndDate] ON [ReadModel].[GroupingReadOnly]
(
	[StartDate] ASC,
	[EndDate] ASC,
	[BusinessUnitId] ASC,
	[LeavingDate] ASC
)
INCLUDE 
(
	PageId, GroupName,GroupId,PersonId,FirstName,LastName,EmploymentNumber,TeamId,SiteId
) 
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)



