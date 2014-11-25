IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_description_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_description_get]
GO


-- report_description_get 1, -1

CREATE   PROCEDURE [mart].[report_description_get]
/*
	Returnerar namnet på rapporten att visa på valsidan
	även namnet som den är sparad under (om något)
	och när den sparades och ändrades
	Ola 2004-10-25
20090211 Added new mart schema KJ
Ola 2008-03-20
tillfälligt utan koll mot sparat
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
exec mart.report_description_get @report_id='19233876-05A6-4198-9B56-30E25EFEC0F3',@saved_name_id=-1
*/
@report_id uniqueidentifier,
@saved_name_id int
AS

SET NOCOUNT ON

DECLARE @name varchar(200)
CREATE TABLE #out (report_name_resource_key nvarchar(50), name nvarchar(500), saved_name nvarchar(200), create_time datetime, modified_time datetime, help_key nvarchar(500))

INSERT into #out
SELECT report_name_resource_key, report_name, '', null, null, help_key
FROM mart.v_report
WHERE Id = @report_id
--
--INSERT INTO @out
--SELECT '', name, create_time, modified_time
--FROM t_webx2_saved_parameters_name
--WHERE id = @saved_name_id
--
--if exists(SELECT 1 FROM t_opti_texts WHERE id = (SELECT text_id FROM t_webx2_report_control_col 
--	WHERE component_id = @component_id) AND culture = @culture)
--	SELECT @name =  t.text
--	FROM t_webx2_report_control_col r
--	INNER JOIN t_opti_texts t ON t.id = r.text_id
--	WHERE component_id = @component_id
--	AND culture = @culture
--
--else
--	SELECT @name =  text
--	FROM t_webx2_report_control_col r
--	INNER JOIN t_opti_texts t ON t.id = r.text_id
--	WHERE component_id = @component_id
--	AND culture = 'en'
--
--if @name is null
--	SELECT @name = 'Text with id ' + convert(varchar,r.text_id) + ' is missing .'
--	FROM t_webx2_report_control_col r
--	WHERE component_id = @component_id
--
--UPDATE @out SET name = @name

SELECT *  FROM #out

GO

