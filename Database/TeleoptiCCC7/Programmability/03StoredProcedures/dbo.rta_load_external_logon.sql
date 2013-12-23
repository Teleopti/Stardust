IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rta_load_external_logon]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[rta_load_external_logon]
GO
--set statistics io on
--set statistics time on
--SELECT businessUnit FROM dbo.PersonTeamSiteBusinessUnit WHERE PersonPeriod='A0CB9940-C33A-4457-9B94-A1CC0101D295'
-- =============================================
-- Author:		David
-- Create date: 2013-12-09
-- Description:	fetches ACD logon original Id for a person period
-- =============================================
-- Change Log
-- Date:		Who:	Description:
--
-- =============================================
--Example call:
--EXEC [dbo].[rta_load_external_logon] '2013-12-12'
CREATE PROCEDURE [dbo].[rta_load_external_logon]
@now datetime
AS

SELECT
	a.Parent as 'person_code',
	el.DataSourceId as 'datasource_id',
	el.AcdLogOnOriginalId as 'acd_login_original_id',
	a.BusinessUnit as 'business_unit_code'
FROM
(
	SELECT
	pp.StartDate,
	pp.Parent,
	pp.PersonPeriod,
	pp.BusinessUnit,
	ROW_NUMBER()OVER(PARTITION BY pp.PersonPeriod ORDER BY pp.StartDate DESC) as is_current
	FROM dbo.v_PersonPeriodTeamSiteBu pp WITH(NOEXPAND) --force SQL Server to use the clustered View
	WHERE pp.StartDate <=  @now --filter out future periods
) a
INNER JOIN dbo.v_ExternalLogon el WITH(NOEXPAND) --force SQL Server to use the clustered View
	ON el.PersonPeriod = a.PersonPeriod
WHERE a.is_current=1