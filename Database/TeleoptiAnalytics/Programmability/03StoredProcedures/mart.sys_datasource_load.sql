IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_datasource_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_datasource_load]
GO

-- =============================================
-- Author:		ChLu
-- Create date: 2008-02-19
-- Description:	Inserts default data sources into sys_datasource.

-- Changed:		DJ
--				Removed static inserts, (default data)
--				Added value to sys_crossdatabaseview [v_log_object]
--				Use the v_sys_datasource for the insert (1:1-mapping so OK)
-- 2009-02-11	Added new mart schema KJ
-- 2010-05-20	Added source id for RTA log object resolve /Robin K
-- 2011-07-19	Dual Agg tables DJ
-- 2011-10-20	collation conflict with old Agg DJ
-- =============================================
CREATE PROCEDURE [mart].[sys_datasource_load] 
	
AS
----------------------------------------------------------------------------
-- Insert agg-database as a data source.
--external log objects
INSERT INTO mart.sys_datasource
	( 
	datasource_name, 
	log_object_id,
	log_object_name,	
	datasource_database_id,
	datasource_database_name,
	datasource_type_name,
	source_id,
	internal
	)
SELECT 
	datasource_name			='Teleopti CCC Agg: '+ lo.log_object_desc Collate Database_Default,
	log_object_id			= lo.log_object_id,
	log_object_name			= lo.log_object_desc Collate Database_Default,
	datasource_database_id	= 2,
	datasource_database_name= 'Teleopti CCC Agg Default',
	datasource_type_name	='Teleopti CCC Agg',
	source_id				= CAST(lo.log_object_id AS NVARCHAR(50)),
	internal				= CASE lo.logDB_Name WHEN db_name() THEN 1 ELSE 0 END
FROM
	mart.v_log_object lo
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource v where v.log_object_id = lo.log_object_id AND datasource_database_id= 2)

--internal qm logs
INSERT INTO mart.sys_datasource
	( 
	datasource_name, 
	log_object_id,
	log_object_name,	
	datasource_database_id,
	datasource_database_name,
	datasource_type_name,
	source_id,
	internal
	)
SELECT 
	datasource_name			= 'Internal Agg: '+ lo.log_object_desc Collate Database_Default,
	log_object_id			= lo.log_object_id,
	log_object_name			= lo.log_object_desc Collate Database_Default,
	datasource_database_id	= 2,
	datasource_database_name= 'TeleoptiAnalytics Default',
	datasource_type_name	= 'Internal Agg' ,
	source_id				= CAST(lo.log_object_id AS NVARCHAR(50)),
	internal				= 1
FROM 
	dbo.log_object lo
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource v where v.log_object_id = lo.log_object_id AND datasource_database_id= 2)

-- Insert queue_original_id = -1 as the default agent queue for each agg datasource_id
-- This id should be excluded from standard queues
INSERT INTO mart.dim_queue_excluded
	(
	queue_original_id,
	datasource_id
	)
SELECT
	queue_original_id	= '-1',
	datasource_id		= ds.datasource_id
FROM
	mart.sys_datasource ds
WHERE  ds.datasource_database_id = 2
AND NOT EXISTS (SELECT * FROM mart.dim_queue_excluded exl WHERE exl.datasource_id	= ds.datasource_id)
GO