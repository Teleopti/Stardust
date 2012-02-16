IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_permission_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_permission_get]
GO


-- =============================================
-- Author:		<KJ
-- Create date: <2008-06-26>
-- 20090211 Added new mart schema KJ
-- Description:	<Checks if a user has permission on a report>
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- EXEC [mart].[report_permission_get] '362951B7-A91E-4212-A343-B36C61DC0108', '362951B7-A91E-4212-A343-B36C61DC0108'
-- =============================================
CREATE PROCEDURE [mart].[report_permission_get]
@person_code uniqueidentifier,
@report_id uniqueidentifier
AS
BEGIN
	
SET NOCOUNT ON

SELECT COUNT(ReportId) report_permission_count 
FROM mart.permission_report
WHERE person_code=@person_code
AND ReportId=@report_id

END

GO

