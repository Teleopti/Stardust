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


IF EXISTS (SELECT 1 FROM #workloads WHERE id = -1)
BEGIN
	--queues not connected will be connected to the 'Not Defined' workload
	DECLARE @maxdate as smalldatetime = CAST('20591231' as smalldatetime)
	INSERT INTO mart.bridge_queue_workload
		( 
		queue_id, 
		workload_id, 
		skill_id,
		business_unit_id,
		datasource_id, 
		insert_date, 
		update_date, 
		datasource_update_date
		)
	SELECT 
		queue_id		= dq.queue_id, 
		workload_id		= -1, 
		skill_id		= -1,
		business_unit_id = -1,
		datasource_id	= -1, 
		insert_date		= getdate(), 
		update_date		= getdate(), 
		datasource_update_date	= isnull(dq.datasource_update_date,@maxdate)
	FROM
		mart.dim_queue	dq
	WHERE queue_id NOT in (select queue_id from mart.bridge_queue_workload)
END

SELECT DISTINCT
	id		= dq.queue_id,
	name	= dq.queue_name
FROM
	mart.dim_queue dq WITH(NOLOCK)
INNER JOIN mart.bridge_queue_workload bqw WITH(NOLOCK)
ON 
	bqw.queue_id=dq.queue_id
WHERE bqw.skill_id in (select id from #skills)
AND bqw.workload_id in (select id from #workloads)
ORDER BY dq.queue_name

GO

