IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WorkAroundFor27636') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.WorkAroundFor27636
GO

CREATE PROCEDURE dbo.WorkAroundFor27636

AS
Set nocount on
delete sar 
	from [dbo].[StudentAvailabilityRestriction] sar
	inner join 
	(
		select [Id],ROW_NUMBER()OVER(PARTITION BY [Person],[RestrictionDate] ORDER BY [UpdatedOn] asc) as 'rn'
		from [dbo].[StudentAvailabilityDay]
	) a
	on a.[Id] = sar.Parent
where a.rn > 1


delete sar 
	from [dbo].[StudentAvailabilityDay] sar
	inner join 
	(
		select [Id],ROW_NUMBER()OVER(PARTITION BY [Person],[RestrictionDate] ORDER BY [UpdatedOn] asc) as 'rn'
		from [dbo].[StudentAvailabilityDay]
	) a
	on a.[Id] = sar.Id
where a.rn > 1

GO