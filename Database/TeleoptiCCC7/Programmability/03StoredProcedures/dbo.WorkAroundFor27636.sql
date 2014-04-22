IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WorkAroundFor27636') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.WorkAroundFor27636
GO

CREATE PROCEDURE dbo.WorkAroundFor27636

AS
Set nocount on
--hourly availability
delete sar 
	from [dbo].[StudentAvailabilityRestriction] sar
	inner join 
	(
		select [Id],ROW_NUMBER()OVER(PARTITION BY [Person],[RestrictionDate] ORDER BY [UpdatedOn] desc) as 'rn'
		from [dbo].[StudentAvailabilityDay]
	) a
	on a.[Id] = sar.Parent
where a.rn > 1


delete sar 
	from [dbo].[StudentAvailabilityDay] sar
	inner join 
	(
		select [Id],ROW_NUMBER()OVER(PARTITION BY [Person],[RestrictionDate] ORDER BY [UpdatedOn] desc) as 'rn'
		from [dbo].[StudentAvailabilityDay]
	) a
	on a.[Id] = sar.Id
where a.rn > 1

--Day Off restriction duplicates
create table #duplicates(Id uniqueidentifier)
insert into #duplicates
select a.Id from
(
	select pd.Id,ROW_NUMBER()OVER(PARTITION BY [Person],[RestrictionDate] ORDER BY [UpdatedOn] desc) as 'rn'
	from dbo.PreferenceDay pd
	inner join dbo.PreferenceRestriction pr
		on pd.Id = pr.Id
	where pr.DayOffTemplate IS NOT NULL
	) a
where a.rn > 1

--Shift category and extended restriction duplicates
union all
select a.Id from
(
	select pd.Id,ROW_NUMBER()OVER(PARTITION BY [Person],[RestrictionDate] ORDER BY [UpdatedOn] desc) as 'rn'
	from dbo.PreferenceDay pd
	inner join dbo.PreferenceRestriction pr
		on pd.Id = pr.Id
	) a
where a.rn > 1

--Absence restriction duplicates
union all
select a.Id from
(
	select pd.Id,ROW_NUMBER()OVER(PARTITION BY [Person],[RestrictionDate] ORDER BY [UpdatedOn] desc) as 'rn'
	from dbo.PreferenceDay pd
	inner join dbo.PreferenceRestriction pr
		on pd.Id = pr.Id
	where pr.Absence IS NOT NULL
	) a
where a.rn > 1

--delete from 3 tables based on #duplicates
delete a
from dbo.ActivityRestriction a
inner join #duplicates t
	on t.Id = a.Parent

delete a
from dbo.PreferenceRestriction a
inner join #duplicates t
	on t.Id = a.Id

delete a
from dbo.PreferenceDay a
inner join #duplicates t
	on t.Id = a.Id

GO
--Execute once upon each patch. Will also be called by ETL via "RemoveDuplicatesWorkaroundFor27636()"
Exec dbo.WorkAroundFor27636