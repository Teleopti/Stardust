IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_permission_report_truncate_nonactive]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_permission_report_truncate_nonactive]
GO


-- =============================================
-- Author:		DJ
-- Create date: 2013-02-08
-- Description:	truncate in a separate batch to avoid lock
-- =============================================
CREATE PROCEDURE [mart].[etl_permission_report_truncate_nonactive]
@business_unit_code uniqueidentifier,
@isFirstBusinessUnit bit,
@isLastBusinessUnit bit
WITH EXECUTE AS OWNER
AS
DECLARE @is_active char(1)
DECLARE @non_active char(1)
SELECT @is_active = is_active FROM [mart].[permission_report_active]

IF @is_active = 'A'
	SET @non_active = 'B'
ELSE
	SET @non_active = 'A'
	
BEGIN
	IF @is_active = 'A'
		TRUNCATE TABLE [mart].[permission_report_B]
	ELSE
		TRUNCATE TABLE [mart].[permission_report_A]
END
GO

