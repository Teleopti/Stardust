IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriodWithEndDate]'))
DROP VIEW [dbo].[PersonPeriodWithEndDate]
GO

CREATE VIEW [dbo].[PersonPeriodWithEndDate]
AS

select pp.*
from personperiod pp
inner join person p on pp.parent = p.id and p.IsDeleted = 0
where pp.StartDate <= isnull(p.terminaldate,'20591231 00:00')
GO