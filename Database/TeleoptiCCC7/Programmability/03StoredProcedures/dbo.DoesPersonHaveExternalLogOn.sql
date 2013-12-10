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
--
-- =============================================
CREATE PROCEDURE [dbo].[DoesPersonHaveExternalLogOn]
@now datetime,
@person uniqueidentifier
AS

declare @PersonPeriod table (Person uniqueidentifier, PersonPeriod uniqueidentifier)

INSERT INTO @PersonPeriod(Person,PersonPeriod)
select
	pp.Parent,
	pp.Id
FROM PersonPeriodWithEndDate pp
WHERE pp.Parent = @Person
AND @now Between pp.StartDate AND pp.EndDate

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
