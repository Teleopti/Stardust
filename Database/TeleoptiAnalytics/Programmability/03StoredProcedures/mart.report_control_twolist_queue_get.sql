IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_queue_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_queue_get]
GO


CREATE Proc [mart].[report_control_twolist_queue_get]
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
as
set nocount on
/*
Create Date: 2008-08-07
Last modified:20080910
20080910 Added parameter @bu_id KJ
20090211 Added new mart schema KJ 
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/
CREATE TABLE #skills(id int)
INSERT INTO #skills
SELECT * FROM SplitStringInt(@skill_set)

CREATE TABLE #workloads(id int)
INSERT INTO #workloads
SELECT * FROM SplitStringInt(@workload_set)

SELECT
	id		= dq.queue_id,
	name	= dq.queue_name
FROM
	mart.dim_queue dq
INNER JOIN mart.bridge_queue_workload bqw ON
	bqw.queue_id=dq.queue_id
WHERE bqw.skill_id in (select id from #skills)
AND bqw.workload_id in (select id from #workloads)
ORDER BY dq.queue_name

GO

