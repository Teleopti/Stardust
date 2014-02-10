IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoesPersonHaveExternalLogOn]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DoesPersonHaveExternalLogOn]
GO

-- =============================================
-- Author:		David
-- Create date: 2013-12-09
-- Description:	fetches ACD logon original Id for a person period
-- =============================================
-- Change Log:
-- Date			Who	Description
-- exec [dbo].[DoesPersonHaveExternalLogOn] '2013-02-12','B0E35119-4661-4A1B-8772-9B5E015B2564'

-- =============================================
CREATE PROCEDURE [dbo].[DoesPersonHaveExternalLogOn]
@now datetime,
@person uniqueidentifier
AS

declare @PersonPeriod table (Person uniqueidentifier, PersonPeriod uniqueidentifier)
INSERT INTO @PersonPeriod(Person,PersonPeriod)
SELECT
	a.Parent,
	a.Id
	FROM
	(
	SELECT
		pp1.Id,
		pp1.StartDate,
		pp1.parent,
		ROW_NUMBER()OVER(PARTITION BY pp1.Parent ORDER BY pp1.Parent ASC,pp1.StartDate ASC) as rn --add a row nuber
	FROM personperiod pp1-- WITH (INDEX(IX_PersonPeriod_Parent_StartDate_Id))
	WHERE pp1.Parent=@person
	) a
	LEFT OUTER JOIN
	(
	SELECT
		pp1.Id,
		pp1.StartDate,
		pp1.parent,
		ROW_NUMBER()OVER(PARTITION BY pp1.Parent ORDER BY pp1.Parent ASC,pp1.StartDate ASC) as rn --add a row nuber
	FROM personperiod pp1-- WITH (INDEX(IX_PersonPeriod_Parent_StartDate_Id))
	WHERE pp1.Parent=@person
	) b
	ON a.rn+1 = b.rn
	AND a.Parent=b.Parent
	WHERE @now between a.StartDate and ISNULL(b.StartDate,'2059-12-31')

--return to client
select
	PP.Person,
	pp.PersonPeriod,
	el.AcdLogOnOriginalId,
	el.DataSourceId
from dbo.externallogoncollection elc
inner join dbo.ExternalLogOn el
	on elc.ExternalLogOn = el.Id
inner join @PersonPeriod pp
	on pp.PersonPeriod = elc.PersonPeriod
GO