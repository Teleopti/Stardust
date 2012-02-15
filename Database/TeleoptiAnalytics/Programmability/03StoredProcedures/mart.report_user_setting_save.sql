IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_user_setting_save]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_user_setting_save]
GO



CREATE  PROCEDURE [mart].[report_user_setting_save]
/*
	Ola 2004-10-26
	Sparar inställningar för en rapport och en användare
	Om man inte sparar en namngiven (dvs vid automatiskt sparande)
	skickas -1 in i @saved_name_id
	20090211 Added new mart schema KJ
	-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/
	@report_id uniqueidentifier,
	@person_code uniqueidentifier,
	@param_name varchar(50),
	@saved_name_id int,
	@setting varchar(MAX)
AS
SET NOCOUNT ON

DECLARE @current_setting varchar(MAX)

SELECT @current_setting = control_setting
FROM mart.report_user_setting
WHERE ReportId = @report_id
AND param_name = @param_name
AND person_code = @person_code
AND saved_name_id = @saved_name_id

IF @current_setting IS NULL
BEGIN
	INSERT INTO mart.report_user_setting
		SELECT @person_code, @report_id, @param_name, @saved_name_id, @setting
END
ELSE
BEGIN
	UPDATE mart.report_user_setting
	SET control_setting = @setting
	WHERE ReportId = @report_id
	AND param_name = @param_name 
	AND person_code = @person_code
	AND saved_name_id = @saved_name_id
END

GO

