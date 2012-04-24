IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_queue_workload_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_queue_workload_load]
GO
/****** Object:  StoredProcedure [mart].[etl_bridge_queue_workload_load]    Script Date: 12/04/2008 15:14:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads bridge, that connects queues with workloads.
-- Update date: 2009-02-11
-- 2009-02-11 New Mart schema KJ
-- 2008-12-04 Bug fix for multi BU KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-04-27 Change dateformat on min/max date DJ
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_queue_workload_load] 
@business_unit_code uniqueidentifier
	
AS

--Create maxdate
DECLARE @maxdate as smalldatetime
SELECT @maxdate=CAST('20591231' as smalldatetime)

/*Get business unit id*/
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

/*Deleting data from bride queue workload */

DELETE FROM mart.bridge_queue_workload 
WHERE business_unit_id = @business_unit_id
OR business_unit_id = -1

/*
DELETE FROM mart.bridge_queue_workload 
WHERE business_unit_id = 
	(
		SELECT DISTINCT
			bu.business_unit_id
		FROM 
			Stage.stg_queue_workload qw
		INNER JOIN
			mart.dim_business_unit bu
		ON
			qw.business_unit_code = bu.business_unit_code
	)
	OR business_unit_id = -1
*/

-- Insert new queues
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
	workload_id		= isnull(dw.workload_id,-1), 
	skill_id		= isnull(dw.skill_id,-1),
	business_unit_id = isnull(dw.business_unit_id,-1),
	datasource_id	= isnull(qw.datasource_id,-1), 
	insert_date		= getdate(), 
	update_date		= getdate(), 
	datasource_update_date	= isnull(qw.datasource_update_date,@maxdate)
FROM
	mart.dim_queue	dq
LEFT JOIN
	Stage.stg_queue_workload qw
ON
	qw.queue_code					= dq.queue_original_id	 
	AND qw.log_object_data_source_id	= dq.datasource_id 
LEFT JOIN
	mart.dim_workload dw
ON 
	qw.workload_code = dw.workload_code
INNER JOIN 
	mart.dim_business_unit bu ON bu.business_unit_id=dw.business_unit_id

--queues not connected
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

GO