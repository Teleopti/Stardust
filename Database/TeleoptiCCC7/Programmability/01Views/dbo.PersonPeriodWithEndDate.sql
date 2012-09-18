IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriodWithEndDate]'))
DROP VIEW [dbo].[PersonPeriodWithEndDate]
GO

CREATE VIEW [dbo].[PersonPeriodWithEndDate]
AS

SELECT pp.*,VirtualEndData.EndDate
FROM PersonPeriod pp
INNER JOIN (
	SELECT 
		ISNULL(personPeriodVirtual2.Id,personPeriodVirtual1.Id) as Id,
		ISNULL(DATEADD(d,-1,personPeriodVirtual1.StartDate),ISNULL(personPeriodVirtual2.TerminalDate,'2059-12-31')) as EndDate,
		personPeriodVirtual2.TerminalDate as TerminalDate2
	FROM 
	(
	SELECT
		pp1.*,
		p.TerminalDate,
		ROW_NUMBER()OVER(PARTITION BY pp1.Parent ORDER BY pp1.Parent,pp1.StartDate) as rn --add a row nuber
	FROM personperiod pp1
	INNER JOIN Person p
	ON pp1.Parent = p.Id
	WHERE ISNULL(p.TerminalDate+1,'2059-12-31') > pp1.StartDate
	) personPeriodVirtual1
	RIGHT OUTER JOIN (
		SELECT
			pp1.Id,
			pp1.Parent,
			pp1.StartDate,
			p.TerminalDate,
			ROW_NUMBER()OVER(PARTITION BY pp1.Parent ORDER BY pp1.Parent,pp1.StartDate) as rn
		FROM personperiod pp1
		INNER JOIN person p
		ON pp1.Parent = p.Id
	) personPeriodVirtual2
	ON personPeriodVirtual1.rn-1 = personPeriodVirtual2.rn
	AND personPeriodVirtual2.parent = personPeriodVirtual1.Parent
) VirtualEndData
	ON VirtualEndData.Id = pp.Id
WHERE VirtualEndData.TerminalDate2+1 > pp.StartDate
