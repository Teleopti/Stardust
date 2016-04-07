IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_get_timezone]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_get_timezone]
GO

-- =============================================
-- Author:		Jonas & maria
-- Create date: 2016-04-07
-- Description:	Inject a time zone and return the same if it exists, else return the default time zone. 
-- Used by Intraday Test tool.
-- =============================================
-- EXEC [mart].[web_intraday_simulator_get_timezone] 'W. Europe Standard Time'
CREATE PROCEDURE [mart].[web_intraday_simulator_get_timezone]
@time_zone_code nvarchar(100)

AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @time_zone nvarchar(100)

	SELECT @time_zone = time_zone_code 
	FROM mart.dim_time_zone 
	WHERE time_zone_code = @time_zone_code

	IF @time_zone IS NULL
	BEGIN
		SELECT @time_zone = time_zone_code 
		FROM mart.dim_time_zone
		WHERE default_zone = 1
	END

	SELECT @time_zone
END

GO

