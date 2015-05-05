IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_intraday_settings_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_job_intraday_settings_load]
GO
-- =============================================
-- Author:		DavidJ
-- Create date: 2014-10-07
-- Description:	Inserts Agg data from log_object_detail into sys_datasource_detail.
-- =============================================
CREATE PROCEDURE [mart].[etl_job_intraday_settings_load] 
	
AS
--REMOVE INVALID LOG_OBJECTS
IF (SELECT COUNT(*) FROM mart.v_log_object_detail)> 0
BEGIN
	DELETE jis
	FROM mart.etl_job_intraday_settings jis
	WHERE NOT EXISTS (SELECT * FROM mart.v_log_object_detail lod 
	INNER JOIN mart.sys_datasource ds
		ON ds.log_object_id = lod.log_object_id
	INNER JOIN mart.etl_job_intraday_settings_type t
		ON t.detail_id = lod.detail_id
	WHERE jis.detail_id=lod.detail_id AND jis.datasource_id=ds.datasource_id)
	AND detail_id IN (1,2)
	--3
	DELETE jis
	FROM mart.etl_job_intraday_settings jis
	WHERE NOT EXISTS (SELECT * FROM mart.v_log_object_detail lod 
	INNER JOIN mart.sys_datasource ds
	ON ds.log_object_id = lod.log_object_id
	WHERE ds.datasource_id = jis.datasource_id and lod.detail_id = 2)
	AND detail_id IN (3)

END
IF (SELECT COUNT(*) FROM mart.v_log_object_detail)= 0 AND (SELECT COUNT(*) FROM dbo.log_object_detail)> 0 --special case when no agg db
BEGIN
	DELETE jis
	FROM mart.etl_job_intraday_settings jis
	WHERE NOT EXISTS (SELECT * FROM dbo.log_object_detail lod 
	INNER JOIN mart.sys_datasource ds
		ON ds.log_object_id = lod.log_object_id
	INNER JOIN mart.etl_job_intraday_settings_type t
		ON t.detail_id = lod.detail_id
	WHERE jis.detail_id=lod.detail_id AND jis.datasource_id=ds.datasource_id)
	AND detail_id IN (1,2)
	--3
	DELETE jis
	FROM mart.etl_job_intraday_settings jis
	WHERE NOT EXISTS (SELECT * FROM dbo.log_object_detail lod 
	INNER JOIN mart.sys_datasource ds
	ON ds.log_object_id = lod.log_object_id
	WHERE ds.datasource_id = jis.datasource_id and lod.detail_id = 2)
	AND detail_id IN (3)
END
--A proper Agg or Azure
insert into [mart].[etl_job_intraday_settings]
select
	business_unit_id	= -1,
	datasource_id		= ds.datasource_id,
	detail_id			= t.detail_id,
	target_date	= DATEADD(DD,-1,(DATEDIFF(DD, 0, getdate()))), --yesterday
	target_interval=0, --start from midnight
	intervals_back=10, --default to 10 intervals back. Limit on bigger customers!
	is_utc = 0
from mart.v_log_object_detail lod
inner join mart.sys_datasource ds
	on ds.log_object_id = lod.log_object_id
inner join mart.etl_job_intraday_settings_type t
	on t.detail_id = lod.detail_id
WHERE NOT EXISTS (SELECT * FROM mart.etl_job_intraday_settings dd where dd.datasource_id = ds.datasource_id and dd.detail_id = lod.detail_id)
AND ds.inactive=0

--When a mix of external and internal Aggs tables (very rare ...)
insert into [mart].[etl_job_intraday_settings]
select
	business_unit_id	= -1,
	datasource_id		= ds.datasource_id,
	detail_id			= t.detail_id,
	target_date	= DATEADD(DD,-1,(DATEDIFF(DD, 0, getdate()))), --yesterday
	target_interval=0, --start from midnight
	intervals_back=10, --default to 10 intervals back. Limit on bigger customers!
	is_utc = 0
from dbo.log_object_detail lod
inner join mart.sys_datasource ds
	on ds.log_object_id = lod.log_object_id
inner join mart.etl_job_intraday_settings_type t
	on t.detail_id = lod.detail_id
WHERE NOT EXISTS (SELECT * FROM mart.etl_job_intraday_settings dd where dd.datasource_id = ds.datasource_id and dd.detail_id = lod.detail_id)
AND ds.inactive=0

--agent Queue mart detail
insert into [mart].[etl_job_intraday_settings]
select
	business_unit_id	= -1,
	datasource_id		= ds.datasource_id,
	detail_id			= t.detail_id,
	target_date	= DATEADD(DD,-1,(DATEDIFF(DD, 0, getdate()))), --yesterday
	target_interval=0, --start from midnight
	intervals_back=10, --default to 10 intervals back. Limit on bigger customers!
	is_utc = 0
from mart.sys_datasource ds
inner join mart.etl_job_intraday_settings_type t
	on t.detail_id = 3
WHERE NOT EXISTS (SELECT * FROM mart.etl_job_intraday_settings dd where dd.datasource_id = ds.datasource_id and dd.detail_id = 3)
AND EXISTS(SELECT * FROM mart.etl_job_intraday_settings dd where dd.datasource_id = ds.datasource_id and dd.detail_id = 2)--AGENT SHOULD EXISTS
AND ds.inactive=0
