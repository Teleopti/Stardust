IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadPersonsWithExternalLogOn]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadPersonsWithExternalLogOn]
GO
--set statistics io on
--set statistics time on
--SELECT businessUnit FROM dbo.PersonTeamSiteBusinessUnit WHERE PersonPeriod='A0CB9940-C33A-4457-9B94-A1CC0101D295'
-- =============================================
-- Author:		David
-- Create date: 2013-12-09
-- Description:	Loads all personIds that are connected to an externalLogOn
-- =============================================
-- Change Log
-- Date:		Who:	Description:
--
-- =============================================
--Example call:
--EXEC dbo.LoadPersonsWithExternalLogOn '2DBE2105-5BF4-4688-B219-A23D0107D273', '2013-11-12'
CREATE PROCEDURE dbo.LoadPersonsWithExternalLogOn
@businessUnitId uniqueidentifier,
@now datetime
AS

SELECT DISTINCT
	a.Parent as 'person_code'
FROM
(
	SELECT
	pp.Parent,
	pp.PersonPeriod,
	ROW_NUMBER()OVER(PARTITION BY pp.PersonPeriod ORDER BY pp.StartDate DESC) as is_current
	FROM dbo.v_PersonPeriodTeamSiteBu pp WITH(NOEXPAND) --force SQL Server to use the clustered View
	WHERE pp.StartDate <=  @now --filter out future periods
	AND pp.BusinessUnit = @businessUnitId
) a
INNER JOIN dbo.v_ExternalLogon el WITH(NOEXPAND) --force SQL Server to use the clustered View
	ON el.PersonPeriod = a.PersonPeriod
WHERE a.is_current=1
