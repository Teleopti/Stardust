IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_skill_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_skill_get]
GO

--EXEC report_control_twolist_skill_get 12,'C04803E2-8D6F-4936-9A90-9B2000148778',1053,'4AD43E49-B233-4D03-A813-9B2000102EBE'

CREATE Proc [mart].[report_control_twolist_skill_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
as
/*
Create Date: 2008-05-28
Last Modified:20090211
	20080910 Added parameter @bu_id KJ
	2090211 Added new mart schema KJ
	20091113 PBI 8456 DJ
	-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/

CREATE TABLE #skills (counter int identity(1,1),id int, name nvarchar(50)) 

--- First "Not Defined"
INSERT #skills(id,name)
SELECT
	id		= skill_id,
	name	= isnull(term_language,skill_name)
FROM
	mart.dim_skill d
LEFT JOIN
	mart.language_translation l
ON
	d.skill_name = l.term_english	AND
	l.language_id = @language_id
WHERE skill_id=-1
ORDER BY isnull(term_language,skill_name)

--and then the rest
INSERT #skills(id,name)
SELECT
	id		= skill_id,
	name	= isnull(term_language,skill_name)
FROM
	mart.dim_skill d
INNER JOIN 
	mart.dim_business_unit b
ON b.business_unit_id=d.business_unit_id
LEFT JOIN
	mart.language_translation l
ON
	d.skill_name = l.term_english	AND
	l.language_id = @language_id
WHERE skill_id<>-1
AND is_deleted=0  --20091113 only active skills
AND b.business_unit_code=@bu_id --20080910 bara det bu som man använder just nu i raptor
ORDER BY isnull(term_language,skill_name)

SELECT id,name
FROM #skills
ORDER BY counter
--select * from dim_skill

GO

