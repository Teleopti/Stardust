IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_datasource_detail_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_datasource_detail_load]
GO

-- =============================================
-- Author:		DavidJ
-- Create date: 2014-10-07
-- Description:	Inserts Agg data from log_object_detail into sys_datasource_detail.
-- =============================================
CREATE PROCEDURE [mart].[sys_datasource_detail_load] 
	
AS
--A proper Agg or Azure
insert into [mart].[sys_datasource_detail]
select
	datasource_id		= ds.datasource_id,
	detail_id			= t.detail_id,
	target_date_local	= d.date_date,
	target_interval_local=0, --start from midnight on best available date
	intervals_back=0 --Delete only current interval and fetch everything matchning that one and new/later intervals from Agg
from mart.v_log_object_detail lod
inner join mart.sys_datasource ds
	on ds.log_object_id = lod.log_object_id
inner join mart.sys_datasource_detail_type t
	on t.detail_id = lod.detail_id
inner join mart.dim_date d
	on DATEDIFF(DD, 0, lod.date_value) = d.date_date
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource_detail dd where dd.datasource_id = ds.datasource_id and dd.detail_id = lod.detail_id)

--When a mix of external and internal Aggs tables (very rare ...)
insert into [mart].[sys_datasource_detail]
select
	datasource_id		= ds.datasource_id,
	detail_id			= t.detail_id,
	target_date_local	= d.date_date,
	target_interval_local=0, --start from midnight on best available date
	intervals_back=0 --Delete only current interval and fetch everything matchning that one and new/later intervals from Agg
from dbo.log_object_detail lod
inner join mart.sys_datasource ds
	on ds.log_object_id = lod.log_object_id
inner join mart.sys_datasource_detail_type t
	on t.detail_id = lod.detail_id
inner join mart.dim_date d
	on DATEDIFF(DD, 0, lod.date_value) = d.date_date
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource_detail dd where dd.datasource_id = ds.datasource_id and dd.detail_id = lod.detail_id)

/* UTC stylish
insert into [mart].[sys_datasource_detail]
select
	datasource_id		= ds.datasource_id,
	detail_id			= 2,
	source_date_id_utc	= -1,
	source_interval_id_utc= -1,
	target_date_id_utc	= max(f.date_id),
	target_interval_id_utc=0
from mart.fact_queue f
inner join mart.dim_queue q
	on q.queue_id = f.queue_id
inner join mart.sys_datasource ds
	on q.datasource_id = ds.datasource_id
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource_detail dd where dd.datasource_id = ds.datasource_id and dd.detail_id = 2)
GROUP BY ds.datasource_id

insert into [mart].[sys_datasource_detail]
select
	datasource_id		= ds.datasource_id,
	detail_id			= 1,
	source_date_id_utc	= -1,
	source_interval_id_utc= -1,
	target_date_id_utc	= max(f.date_id),
	target_interval_id_utc=0
from mart.fact_agent f
inner join mart.dim_acd_login a
	on a.acd_login_id = f.acd_login_id
inner join mart.sys_datasource ds
	on a.datasource_id = ds.datasource_id
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource_detail dd where dd.datasource_id = ds.datasource_id and dd.detail_id = 1)
GROUP BY ds.datasource_id
*/