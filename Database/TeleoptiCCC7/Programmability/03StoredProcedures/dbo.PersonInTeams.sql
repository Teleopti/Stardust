IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PersonInTeams]')
   AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[PersonInTeams]
GO

CREATE PROCEDURE [dbo].[PersonInTeams]
	@belongs_to_date datetime,
	@team_ids nvarchar(max)
AS
BEGIN

SET NOCOUNT ON;


CREATE TABLE #TeamIds (tId uniqueidentifier NOT NULL)

DECLARE @belongs_to_date_ISO nvarchar(10)

SELECT @belongs_to_date_ISO = CONVERT(NVARCHAR(10), @belongs_to_date,120)
    
INSERT INTO #TeamIds SELECT * FROM dbo.SplitStringString(@team_ids)

SELECT DISTINCT p.Id FROM #TeamIds t 
INNER JOIN dbo.PersonPeriod pp with (nolock) ON t.tId = pp.Team 
INNER JOIN dbo.Person p with (nolock) ON pp.Parent = p.Id 
WHERE ISNULL(p.TerminalDate, '2100-01-01') >=  @belongs_to_date_ISO  
AND p.IsDeleted = 0
AND (pp.StartDate IS NULL OR pp.StartDate <=  @belongs_to_date_ISO  ) 
AND (pp.EndDate IS NULL OR pp.EndDate >=  @belongs_to_date_ISO )

END
GO
