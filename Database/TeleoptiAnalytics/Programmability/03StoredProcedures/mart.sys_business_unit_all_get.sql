IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_business_unit_all_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_business_unit_all_get]
GO

-- =============================================
-- Author:		Jonas n
-- Create date: 2012-05-08
-- Description:	Return list of business units inlcuding the 'All' item at top.
-- =============================================
CREATE PROCEDURE [mart].[sys_business_unit_all_get]
AS
BEGIN
	SET NOCOUNT ON;

    CREATE TABLE #bu	
					(
						business_unit_code uniqueidentifier, 
						business_unit_name nvarchar(100)
					)
	INSERT INTO #bu
	SELECT '00000000-0000-0000-0000-000000000002', '<All>'
	
	INSERT INTO #bu
	SELECT
		business_unit_code,
		business_unit_name
	FROM mart.dim_business_unit
	WHERE business_unit_code IS NOT NULL
	ORDER BY business_unit_name
	
	SELECT
		business_unit_code AS id,
		business_unit_name AS name
	FROM #bu
END

GO


