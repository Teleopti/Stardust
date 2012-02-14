IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_configuration_save]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_configuration_save]
GO

CREATE PROCEDURE [mart].[sys_configuration_save]
	@key nvarchar(50),
	@value nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM mart.sys_configuration
	WHERE [key] = @key

    INSERT INTO mart.sys_configuration ([key], [value])
    VALUES (@key, @value)
END

GO


