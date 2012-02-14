IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_workload_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_workload_get]
GO


CREATE Proc [mart].[report_control_workload_get]
@report_id int,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@skill_set nvarchar(max),--Kj lista på skills
@bu_id uniqueidentifier
as
/*
Last Modified: 20090211 KJ
	20080910 Added parameter @bu_id KJ
	20090211 Added new mart schema KJ
	20091114 Added Is_deleted filter DJ
*/

CREATE TABLE #workloads (counter int identity(1,1),id int, name nvarchar(50)) 
CREATE TABLE #skills(id int)
INSERT INTO #skills
SELECT * FROM SplitStringInt(@skill_set)


DECLARE @skill_id int
SET @skill_id= (select id from #skills where id=-2)


--Load "NotDefined" first
INSERT #workloads(id,name)
SELECT
	id		= workload_id,
	name	= isnull(term_language,workload_name)
FROM
	mart.dim_workload d
LEFT JOIN
	mart.language_translation l
ON
	d.workload_name = l.term_english	AND
	l.language_id = @language_id
WHERE 
	(d.skill_id IN (select id from #skills) OR @skill_id =-2)
AND workload_id=-1  --Not Defined
ORDER BY isnull(term_language,workload_name)


INSERT #workloads(id,name)
SELECT
	id		= workload_id,
	name	= isnull(term_language,workload_name)
FROM
	mart.dim_workload d
LEFT JOIN
	mart.language_translation l
ON
	d.workload_name = l.term_english	AND
	l.language_id = @language_id
WHERE 
	(d.skill_id IN (select id from #skills) OR @skill_id =-2)
AND workload_id <>-1  --The rest
AND	is_deleted =0 --20091113 only active workloads.
ORDER BY isnull(term_language,workload_name)

SELECT id,name
FROM #workloads
ORDER BY counter

GO

