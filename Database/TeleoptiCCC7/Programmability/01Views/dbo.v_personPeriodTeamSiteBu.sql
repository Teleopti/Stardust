--Needed for clustered index on view
SET NUMERIC_ROUNDABORT OFF;
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT,
    QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_PersonPeriodTeamSiteBu]'))
DROP VIEW [dbo].[v_PersonPeriodTeamSiteBu]
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
CREATE VIEW [dbo].[v_PersonPeriodTeamSiteBu]
WITH SCHEMABINDING
AS

SELECT
	pp.Id as 'PersonPeriod',
	pp.StartDate as 'StartDate',
	pp.Parent as 'Parent',
	bu.Id as 'BusinessUnit',
	s.Id as 'Site',
	t.Id as 'Team'
FROM dbo.PersonPeriod pp
INNER JOIN dbo.Team t
	ON pp.Team = t.Id
INNER JOIN dbo.Site s
	ON t.Site = s.Id
INNER JOIN dbo.BusinessUnit bu
	ON s.BusinessUnit = bu.Id
GO

CREATE UNIQUE CLUSTERED INDEX CIX_v_PersonPeriodTeamSiteBu
    ON dbo.v_PersonPeriodTeamSiteBu ([PersonPeriod] ASC,[StartDate] DESC);
GO