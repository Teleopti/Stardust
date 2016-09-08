--Top index needs at Dish

--PK with PersonId supports DELETE, this one can support selects
CREATE NONCLUSTERED INDEX IX_GroupingReadOnly_GroupId_PageIdStartDate_EndDate ON [ReadModel].[GroupingReadOnly] ( [GroupId], [PageId],[StartDate], [EndDate] ) INCLUDE ([PersonId])

--Makes sense
CREATE NONCLUSTERED INDEX IX_PersonAvailability_Person_IsDeleted_BusinessUnit ON [dbo].[PersonAvailability] ( [Person], [IsDeleted], [BusinessUnit] )
