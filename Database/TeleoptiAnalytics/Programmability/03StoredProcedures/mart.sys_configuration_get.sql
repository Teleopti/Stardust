IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_configuration_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_configuration_get]
GO


CREATE PROCEDURE [mart].[sys_configuration_get]
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		[key],
		value
    FROM mart.sys_configuration
    ORDER BY [key]
END

GO


