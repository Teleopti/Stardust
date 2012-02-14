IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_scenario_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_scenario_get]
GO

--[mart].[report_control_scenario_get] NULL,NULL,NULL,'928DD0BC-BF40-412E-B970-9B5E015AADEA'
-- =============================================
-- Author:		KJ
-- Create date: 2008-05-28
-- Description:	Loads scenarios to report control cboScenario

-- Change Log
-- Date			Author	Description
-- 20100218		...		Get default_scenario=1 first (instead of name='default')
-- 20090211		KJ		Added new mart schema
-- 20080910		KJ		Added parameter @bu_id
-- 20100727		DJ		Added hard coded filter: is_deleted
-- =============================================
CREATE PROCEDURE [mart].[report_control_scenario_get] 
@report_id int,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id			uniqueidentifier
AS

CREATE TABLE #scenarios (counter int identity(1,1),id int, name nvarchar(50)) 
/* Ska vi verkligen ha med denna???
--Load "NotDefined" first
INSERT #scenarios(id,name)
SELECT
	id		= scenario_id,
	name	= isnull(term_language,scenario_name)
FROM
	dim_scenario d
LEFT JOIN
	mart.language_translation l
ON
	d.scenario_name = l.term_english	AND
	l.language_id = @language_id
WHERE 
	scenario_id=-1
ORDER BY isnull(term_language,scenario_name)
*/

INSERT #scenarios(id,name)
SELECT
	id		= scenario_id,
	name	= scenario_name
FROM
	mart.dim_scenario d
WHERE 
	--scenario_name='Default'
	default_scenario = 1 --Always load current default_scenario (regardless of is_deleted)
AND d.business_unit_code=@bu_id --20080910 bara det bu som man använder just nu i raptor
ORDER BY scenario_name


INSERT #scenarios(id,name)
SELECT
	id		= scenario_id,
	name	= scenario_name
FROM
	mart.dim_scenario d
WHERE 
	scenario_id <>-1
	--AND scenario_name<>'Default'
	AND default_scenario <> 1
	AND is_deleted = 0 --2010-07-27 Load only active scenarios
AND d.business_unit_code=@bu_id --20080910 bara det bu som man använder just nu i raptor
ORDER BY scenario_name

SELECT id,name
FROM #scenarios
ORDER BY counter



GO

