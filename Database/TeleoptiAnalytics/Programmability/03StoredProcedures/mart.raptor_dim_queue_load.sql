IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_dim_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_dim_queue_load]
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2009-02-02>
-- Update date :<2009-02-02>
-- Description:	<<File Import - Loads data to dim_queue from stg_queue>
-- =============================================
CREATE PROCEDURE [mart].[raptor_dim_queue_load] 
WITH EXECUTE AS OWNER
AS
BEGIN
--DECLARE
DECLARE @datasource_id smallint
DECLARE @log_object_name nvarchar(100)

--INIT
SET @datasource_id = 1
SET @log_object_name  = 'Teleopti CCC - File import'

--------------------------------------------------------------
-- Insert Not Defined queue
SET IDENTITY_INSERT mart.dim_queue ON
INSERT INTO mart.dim_queue
	(
	queue_id,
	queue_name,
	datasource_id
	)
SELECT 
	queue_id			=-1,
	queue_name			='Not Defined',
	datasource_id		=-1
WHERE NOT EXISTS (SELECT * FROM mart.dim_queue where queue_id = -1)
SET IDENTITY_INSERT mart.dim_queue OFF

--Update
--Existing queues with a queue_original_id in the importfile
UPDATE mart.dim_queue
SET
	queue_original_id	= stg.queue_code, 
	queue_name			= stg.queue_name,
	log_object_name		= @log_object_name, 
	update_date			= getdate()
FROM
	mart.v_stg_queue stg	
JOIN
	mart.dim_queue q
ON
	q.queue_original_id	= stg.queue_code 			AND
	q.datasource_id		= @datasource_id
WHERE stg.queue_code IS NOT NULL

--------------------------------------------------------------
--Update
--Existing queues without a queue_original_id (fallback on queue_name)
UPDATE mart.dim_queue
SET 
	queue_original_id	= q.queue_id, 
	queue_name			= stg.queue_name,
	log_object_name		= @log_object_name, 
	update_date			= getdate()
FROM
	mart.v_stg_queue stg	
JOIN
	mart.dim_queue q
ON
	q.queue_name		= stg.queue_name 			AND
	q.datasource_id		= @datasource_id
WHERE stg.queue_code	IS NULL

---------------------------------------------------------------------------
-- Insert new queues with a queue_original_id
INSERT INTO mart.dim_queue
	( 
	queue_agg_id,
	queue_original_id, 
	queue_name,
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_original_id		= stg.queue_code, 
	queue_name				= max(stg.queue_name),
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	mart.v_stg_queue stg
WHERE 
	NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
					WHERE	d.queue_original_id= stg.queue_code 	AND
							d.datasource_id =@datasource_id
				)
AND stg.queue_code IS NOT NULL
GROUP BY stg.queue_code

----------------------------------------------------------------------------------------
-- Insert new queues without a queue_original_id (fallback on queue_name)
INSERT INTO mart.dim_queue
	( 
	queue_agg_id,
	queue_original_id, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_original_id		= NULL, 
	queue_name				= stg.queue_name,
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	mart.v_stg_queue stg
WHERE 
	NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
					WHERE	d.queue_name= stg.queue_name 	AND
							d.datasource_id =@datasource_id
				)
AND stg.queue_code IS NULL
GROUP BY stg.queue_name

--SET queue_agg_id AND queue_original_id TO SAME VALUES AS queue_id IF NO queue_original_id OR queue_agg_id
UPDATE mart.dim_queue
SET queue_agg_id=queue_id, queue_original_id= queue_id
WHERE queue_agg_id IS NULL 
AND queue_original_id IS NULL
AND datasource_id=@datasource_id

UPDATE mart.dim_queue
SET queue_agg_id=queue_original_id
WHERE queue_agg_id IS NULL 
AND queue_original_id IS NOT NULL
AND datasource_id=@datasource_id

--Update queue_description if IS NULL
UPDATE mart.dim_queue
SET queue_description = queue_name
WHERE queue_description IS NULL
AND datasource_id=@datasource_id --only for File Imports!

END
GO
