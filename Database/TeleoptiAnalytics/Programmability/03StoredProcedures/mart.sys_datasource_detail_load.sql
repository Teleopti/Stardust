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
	target_date_local	= DATEADD(DD,-1,(DATEDIFF(DD, 0, getdate()))), --yesterday
	target_interval_local=0, --start from midnight
	intervals_back=0 --Delete only current interval and fetch everything matchning that one and new/later intervals from Agg
from mart.v_log_object_detail lod
inner join mart.sys_datasource ds
	on ds.log_object_id = lod.log_object_id
inner join mart.sys_datasource_detail_type t
	on t.detail_id = lod.detail_id
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource_detail dd where dd.datasource_id = ds.datasource_id and dd.detail_id = lod.detail_id)
AND ds.inactive=0

--When a mix of external and internal Aggs tables (very rare ...)
insert into [mart].[sys_datasource_detail]
select
	datasource_id		= ds.datasource_id,
	detail_id			= t.detail_id,
	target_date_local	= DATEADD(DD,-1,(DATEDIFF(DD, 0, getdate()))), --yesterday
	target_interval_local=0, --start from midnight
	intervals_back=0 --Delete only current interval and fetch everything matchning that one and new/later intervals from Agg
from dbo.log_object_detail lod
inner join mart.sys_datasource ds
	on ds.log_object_id = lod.log_object_id
inner join mart.sys_datasource_detail_type t
	on t.detail_id = lod.detail_id
WHERE NOT EXISTS (SELECT * FROM mart.sys_datasource_detail dd where dd.datasource_id = ds.datasource_id and dd.detail_id = lod.detail_id)
AND ds.inactive=0
GO