IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_group_page_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_group_page_get]
GO

-- =============================================
-- Author:		HG
-- Create date: 2010-08-26
-- Description:	Loads GroupPages to report control cboGroupPage

-- EXEC [mart].[report_control_group_page_get] @report_id=13,@person_code='B0E35119-4661-4A1B-8772-9B5E015B2564',@language_id=1025,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'
-- Change Log
-- Date			Author	Description
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROCEDURE [mart].[report_control_group_page_get] 
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier,
@business_hierarchy_code uniqueidentifier
AS

DECLARE @empty_guid uniqueidentifier
SET @empty_guid = '00000000-0000-0000-0000-000000000000'

CREATE TABLE #group_page
(
	id uniqueidentifier,
	name nvarchar(100),
	group_order int,
	group_page_name_resource_key nvarchar(100)
)

-- Insert the Main (Business Hierarchy) group page
INSERT INTO #group_page
SELECT DISTINCT
		id								= @business_hierarchy_code, -- hardcoded id provided from asp.net code
		name							= NULL,
		group_order						= -1,
		group_page_name_resource_key	= 'Main'


-- Insert built in group pages
INSERT INTO #group_page
SELECT DISTINCT
		id								= group_page_code,
		name							= NULL,
		group_order						= 0,
		group_page_name_resource_key	= group_page_name_resource_key
	FROM
		mart.dim_group_page
	WHERE
		business_unit_code = @bu_id AND
		group_page_name_resource_key IS NOT NULL
		AND group_code <> @empty_guid


-- Translate group page name
UPDATE #group_page
SET
	name	= isnull(l.term_language,l2.term_english)
FROM 
	#group_page gp
LEFT JOIN
	mart.language_translation l
ON
	l.ResourceKey = gp.group_page_name_resource_key COLLATE database_default
	AND
	l.language_id = @language_id
LEFT JOIN
	mart.language_translation l2
ON
	l2.ResourceKey = gp.group_page_name_resource_key COLLATE database_default


-- Insert user defined group pages
INSERT INTO #group_page
SELECT DISTINCT
		id				= group_page_code,
		name			= group_page_name,
		group_order		= 1,
		NULL
	FROM
		mart.dim_group_page
	WHERE
		business_unit_code = @bu_id AND
		group_page_name_resource_key IS NULL


SELECT 
	id,
	name
FROM #group_page
ORDER BY group_order, name
GO