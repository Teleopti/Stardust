IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadAllPersonsCurrentBuSiteTeam]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadAllPersonsCurrentBuSiteTeam]
GO
-- =============================================
-- Author:		David
-- Create date: 2014-03-07
-- Description:	fetches all person with current team, site, bu based on "now" as input
-- =============================================
-- Change Log
-- Date:		Who:	Description:
--
-- =============================================
--Example call:
--EXEC [dbo].[LoadAllPersonsCurrentBuSiteTeam] '2013-12-12'
CREATE PROCEDURE [dbo].[LoadAllPersonsCurrentBuSiteTeam]
@now datetime
AS

SELECT
	a.Parent as 'PersonId',
--	el.DataSourceId as 'datasource_id',  --CTI stuff. Enable theese to columns to fetch CTI/acd-login at the same time. Note: person:externalLogon is n:n!
--	el.AcdLogOnOriginalId as 'acd_login_original_id', --CTI stuff
	a.Site as 'SiteId',
	a.Team as 'TeamId'
FROM
(
	SELECT
	pp.StartDate,
	pp.Parent,
	pp.PersonPeriod,
	pp.BusinessUnit,
	pp.Site,
	pp.Team,
	ROW_NUMBER()OVER(PARTITION BY pp.PersonPeriod ORDER BY pp.StartDate DESC) as is_current
	FROM dbo.v_PersonPeriodTeamSiteBu pp WITH(NOEXPAND) --force SQL Server to use the clustered View
	WHERE pp.StartDate <=  @now --filter out future periods
) a
--INNER JOIN dbo.v_ExternalLogon el WITH(NOEXPAND) --force SQL Server to use the clustered View
--	ON el.PersonPeriod = a.PersonPeriod
WHERE a.is_current=1