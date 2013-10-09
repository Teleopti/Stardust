IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_permission_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_permission_get]
GO


-- =============================================
-- Author:		<KJ
-- Create date: <2008-06-26>
-- 20090211 Added new mart schema KJ
-- Description:	<Checks if a user has permission on a report>
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- EXEC [mart].[report_permission_get] '3F0886AB-7B25-4E95-856A-0D726EDC2A67', '71BDB56D-C12F-489B-8275-04873A668D90'
-- EXEC [mart].[report_permission_get] '3F0886AB-7B25-4E95-856A-0D726EDC2A67', 'F7F3AF97-EC24-4EA8-A2C7-5175879C7ACC'
-- =============================================
CREATE PROCEDURE [mart].[report_permission_get]
@person_code uniqueidentifier,
@report_id uniqueidentifier
AS
BEGIN
	
SET NOCOUNT ON
	
DECLARE @HasPermission int

--Visable reports that do have permissions in domain
SELECT @HasPermission=COUNT(ReportId)
FROM mart.permission_report
WHERE person_code=@person_code
AND ReportId=@report_id

--Hidden reports, just show them if the user know the URL
SELECT @HasPermission = @HasPermission + COUNT(Id)
FROM mart.report
WHERE visible=0
AND Id=@report_id

--return
SELECT @HasPermission as report_permission_count 

END

GO

