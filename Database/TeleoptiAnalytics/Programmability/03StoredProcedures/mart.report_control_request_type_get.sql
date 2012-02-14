IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_request_type_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_request_type_get]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2012-01-21
-- Description:	Get request types + "all" from dim_request
------------------------------------------------
-- Change Log
-- Date			Author	Description
------------------------------------------------
-- yyyy-mm-dd	AB		some change
-- =============================================
--exec [mart].[report_control_request_type_get] @report_id=1,@person_code='8024B280-4698-4DE1-8E7C-EE162E6A1A75',@language_id=1033,@bu_id='8024B280-4698-4DE1-8E7C-EE162E6A1A75'

CREATE PROCEDURE [mart].[report_control_request_type_get] 
@report_id int,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

CREATE TABLE #sites (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT #sites(id,name)
SELECT
	id		= -2,
	name	= 'All'	

UPDATE #sites
SET name=l.term_language
FROM 
	mart.language_translation l
WHERE #sites.name=l.term_english COLLATE database_default
AND l.language_id = @language_id


INSERT #sites(id,name)
SELECT 
rt.request_type_id,
ISNULL(typeTranslations.term_language, rt.request_type_name)
FROM
		mart.dim_request_type rt
left outer join mart.language_translation typeTranslations
	on typeTranslations.ResourceKey = rt.resource_key COLLATE database_default
	and typeTranslations.language_id=@language_id
	
SELECT * FROM #sites