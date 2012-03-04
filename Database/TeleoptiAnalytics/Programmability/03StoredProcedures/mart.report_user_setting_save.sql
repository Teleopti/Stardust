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
	exec mart.report_user_setting_save @person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id='0E3F340F-C05D-4A98-AD23-A019607745C9',@param_name=N'@date_from',@saved_name_id=-1,@setting=N'2012-02-16 00:00:00'
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
		SELECT @report_id, @person_code, @param_name, @saved_name_id, @setting
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

