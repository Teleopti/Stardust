IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_intraday_settings_load_deviation]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_job_intraday_settings_load_deviation]
GO
-- =============================================
-- Author:		DavidJ
-- Create date: 2014-12-15
-- Description:	Inserts none existing BU into [mart].[etl_job_intraday_settings]
-- =============================================
CREATE PROCEDURE [mart].[etl_job_intraday_settings_load_deviation]
@business_unit_code uniqueidentifier
AS

--deviation
insert into [mart].[etl_job_intraday_settings]
select
	business_unit_id	= business_unit_id,
	datasource_id		= -1,
	detail_id			= t.detail_id,
	target_date	= DATEADD(DD,-1,(DATEDIFF(DD, 0, getdate()))), --yesterday
	target_interval=0, --start from midnight
	intervals_back=10, --default to 10 intervals back. Limit on bigger customers!
	is_utc = 0
FROM mart.dim_business_unit b
inner join mart.etl_job_intraday_settings_type t
	on t.detail_id = 4
WHERE NOT EXISTS (SELECT * FROM mart.etl_job_intraday_settings dd where dd.business_unit_id = b.business_unit_id and dd.detail_id = 4)
AND b.business_unit_id > -1
