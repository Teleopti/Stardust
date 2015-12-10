--Example call:
--EXEC [dbo].[LoadAllPersonOrganizationData] '2015-12-02'

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadAllPersonOrganizationData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadAllPersonOrganizationData]
GO

CREATE PROCEDURE [dbo].[LoadAllPersonOrganizationData]
@now datetime
AS

SELECT
	v.PersonId,
	v.TeamId,
	v.SiteId,
	v.BusinessUnitId
FROM
	dbo.v_PersonOrganizationData v WITH(NOEXPAND)
WHERE
	@now BETWEEN v.StartDate AND v.EndDate

GO
