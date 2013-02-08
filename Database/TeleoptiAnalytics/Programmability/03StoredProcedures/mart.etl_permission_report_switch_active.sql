IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_permission_report_switch_active]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_permission_report_switch_active]
GO


-- =============================================
-- Author:		DJ
-- Create date: 2013-02-08
-- Description:	truncate in a separate batch to avoid lock
-- =============================================
CREATE PROCEDURE [mart].[etl_permission_report_switch_active]
@business_unit_code uniqueidentifier,
@isFirstBusinessUnit bit,
@isLastBusinessUnit bit
AS
	DECLARE @is_active char(1)
	SELECT @is_active = is_active FROM [mart].[permission_report_active]

	--If "LastBU", then switch active table
	IF @isLastBusinessUnit = 1
	BEGIN
		IF @is_active = 'A'
			UPDATE [mart].[permission_report_active]
			SET is_active = 'B'
		ELSE
			UPDATE [mart].[permission_report_active]
			SET is_active = 'A'
	END

GO

