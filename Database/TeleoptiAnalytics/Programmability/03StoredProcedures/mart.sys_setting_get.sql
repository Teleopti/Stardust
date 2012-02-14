IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_setting_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_setting_get]
GO


-- =============================================
-- Author:		jonas n
-- Create date: 2011-11-16
-- Description:	Get a setting matching the given key parameter
-- =============================================
CREATE PROCEDURE [mart].[sys_setting_get]
	@key nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[key],
		value
	FROM
		mart.sys_setting
	WHERE
		[key] = @key
END

GO


