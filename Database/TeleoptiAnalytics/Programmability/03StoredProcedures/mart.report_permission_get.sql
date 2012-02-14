IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_permission_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_permission_get]
GO


-- =============================================
-- Author:		<KJ
-- Create date: <2008-06-26>
-- 20090211 Added new mart schema KJ
-- Description:	<Checks if a user has permission on a report>
-- =============================================
CREATE PROCEDURE [mart].[report_permission_get]
@person_code uniqueidentifier,
@report_id int
AS
BEGIN
	
SET NOCOUNT ON

SELECT COUNT(report_id) report_permission_count 
FROM mart.permission_report
WHERE person_code=@person_code
AND report_id=@report_id

END

GO

