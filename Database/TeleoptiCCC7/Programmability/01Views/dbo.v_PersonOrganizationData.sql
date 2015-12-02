--Needed for clustered index on view
SET NUMERIC_ROUNDABORT OFF;
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT,
    QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_PersonOrganizationData]'))
DROP VIEW [dbo].[v_PersonOrganizationData]
GO

CREATE VIEW [dbo].[v_PersonOrganizationData]
WITH SCHEMABINDING
AS

SELECT
	pp.Id as 'PersonPeriodId',
	el.AcdLogOnOriginalId as 'AcdLogOnOriginalId',
	el.DataSourceId as 'DataSourceId',
	pp.StartDate as 'StartDate',
	pp.EndDate as 'EndDate',
	p.Id as 'PersonId',
	pp.Team as 'TeamId',
	t.Site AS 'SiteId',
	s.BusinessUnit as 'BusinessUnitId'
FROM
	dbo.Person p
JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
JOIN dbo.ExternalLogOnCollection pc ON pc.PersonPeriod = pp.Id
JOIN dbo.ExternalLogOn el ON el.Id = pc.ExternalLogOn
JOIN dbo.Team t ON t.Id = pp.Team
JOIN dbo.Site s ON s.Id = t.Site

GO

CREATE UNIQUE CLUSTERED INDEX CIX_v_LoadPersonData
    ON dbo.[v_PersonOrganizationData] (PersonPeriodId, AcdLogOnOriginalId ASC, StartDate DESC);
GO