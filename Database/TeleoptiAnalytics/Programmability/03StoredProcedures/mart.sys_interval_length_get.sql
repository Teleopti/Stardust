IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_interval_length_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_interval_length_get]
GO


CREATE PROCEDURE [mart].[sys_interval_length_get]
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		CAST([value] AS INT)
    FROM mart.sys_configuration
    WHERE [key] = 'IntervalLengthMinutes'
END

GO


