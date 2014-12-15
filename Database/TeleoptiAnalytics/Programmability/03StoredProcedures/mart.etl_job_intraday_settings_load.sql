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
