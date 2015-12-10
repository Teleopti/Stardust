--Example call:
--EXEC [dbo].[LoadPersonOrganizationData] '2015-12-02', '6', '200569'

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadPersonOrganizationData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadPersonOrganizationData]
GO

CREATE PROCEDURE [dbo].[LoadPersonOrganizationData]
@now datetime,
@dataSourceId int,
@externallogon varchar(50)
AS

SELECT
	v.PersonId,
	v.TeamId,
	v.SiteId,
	v.BusinessUnitId
FROM
	dbo.v_PersonOrganizationData v WITH(NOEXPAND)
WHERE
	v.AcdLogOnOriginalId = @externallogon AND
	v.DataSourceId = @dataSourceId AND 
	@now BETWEEN v.StartDate AND v.EndDate

GO
