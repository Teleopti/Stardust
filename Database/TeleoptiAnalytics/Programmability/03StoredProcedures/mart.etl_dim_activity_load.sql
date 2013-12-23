IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_activity_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_activity_load]
GO
/****** Object:  StoredProcedure [mart].[etl_dim_activity_load]    Script Date: 12/04/2008 15:30:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Update date: 2012-11-21
-- 2012-11-21 New column display_color KJ
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2008-12-01 Added new fields in_ready_time_name, in_contract_time,in_contract_time_name,in_paid_time,in_pad_time_name,in_work_time,in_work_time_name KJ
-- 2008-12-04 Bug fix KJ
-- Description:	Loads activities from stg_activity to dim_activity.

-- =============================================
CREATE PROCEDURE [mart].[etl_dim_activity_load] 
@business_unit_code uniqueidentifier		
WITH EXECUTE AS OWNER
AS
--------------------------------------------------------------------------
-- Not Defined Activity
SET IDENTITY_INSERT mart.dim_activity ON

INSERT INTO mart.dim_activity
	(
	activity_id,
	activity_code,
	activity_name, 
	display_color,
	in_ready_time,
	in_ready_time_name,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,
	is_deleted, 
	display_color_html
	)
SELECT 
	activity_id				= -1, 
	activity_code			= '00000000-0000-0000-0000-000000000000',
	activity_name			= 'Not Defined', 
	display_color			= -1,
	in_ready_time			= 0,
	in_ready_time_name		= 'Not In Ready Time',
	in_contract_time		= 0,
	in_contract_time_name	= 'Not In Contract Time',
	in_paid_time			= 0,
	in_paid_time_name		= 'Not In Paid Time',
	in_work_time			= 0,
	in_work_time_name		= 'Not In Work Time',
	is_deleted				= 0,
	display_color_html		= '#FFFFFF'
WHERE NOT EXISTS (SELECT * FROM mart.dim_activity where activity_id = -1)

SET IDENTITY_INSERT mart.dim_activity OFF

CREATE TABLE #tmp(ready_bit bit,ready_description nvarchar(50),contract_bit bit, contract_description nvarchar(50),paid_bit bit, paid_description nvarchar(50),work_bit bit, work_description nvarchar(50))
INSERT #tmp(ready_bit,ready_description,contract_bit,contract_description,paid_bit,paid_description,work_bit,work_description) SELECT 0, 'Not In Ready Time', 0,'Not In Contract Time',0,'Not In Paid Time',0,'Not In Work Time'
INSERT #tmp(ready_bit,ready_description,contract_bit,contract_description,paid_bit,paid_description,work_bit,work_description) SELECT 1,'In Ready Time', 1,'In Contract Time',1,'In Paid Time',1,'In Work Time'

---------------------------------------------------------------------------
-- update changes on activities
UPDATE mart.dim_activity
SET 
	activity_name			= s.activity_name,
	display_color			= s.display_color,
	in_ready_time			= s.in_ready_time,
	in_ready_time_name		= t1.ready_description,
	in_contract_time		= s.in_contract_time,
	in_contract_time_name	= t2.contract_description,
	in_paid_time			= s.in_paid_time,
	in_paid_time_name		= t3.paid_description ,
	in_work_time			= s.in_work_time,
	in_work_time_name		= t4.work_description,
	is_deleted				= s.is_deleted, 
	display_color_html		= s.display_color_html
FROM
	Stage.stg_activity s
LEFT JOIN 
	#tmp t1 ON s.in_ready_time=t1.ready_bit 
LEFT JOIN 
	#tmp  t2 ON s.in_contract_time=t2.contract_bit 
LEFT JOIN 
	#tmp t3 ON s.in_paid_time=t3.paid_bit
LEFT JOIN 
	#tmp t4 ON s.in_work_time=t4.work_bit
WHERE 
	s.activity_code = mart.dim_activity.activity_code

-- Insert new activities
INSERT INTO mart.dim_activity
	(
	activity_code, 
	activity_name, 
	display_color,
	in_ready_time,
	in_ready_time_name,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,
	business_unit_id,
	is_deleted, 
	display_color_html
	)
SELECT 
	activity_code			= s.activity_code, 
	activity_name			= s.activity_name, 
	display_color			= s.display_color,
	in_ready_time			= s.in_ready_time,
	in_ready_time_name		= t1.ready_description,
	in_contract_time		= s.in_contract_time,
	in_contract_time_name	= t2.contract_description,
	in_paid_time			= s.in_paid_time,
	in_paid_time_name		= t3.paid_description ,
	in_work_time			= s.in_work_time,
	in_work_time_name		= t4.work_description,
	business_unit_id		= bu.business_unit_id,
	is_deleted				= s.is_deleted, 
	display_color_html		= s.display_color_html
FROM
	Stage.stg_activity s
JOIN
	mart.dim_business_unit bu
ON
	s.business_unit_code = bu.business_unit_code
LEFT JOIN 
	#tmp t1 ON s.in_ready_time=t1.ready_bit 
LEFT JOIN 
	#tmp  t2 ON s.in_contract_time=t2.contract_bit 
LEFT JOIN 
	#tmp t3 ON s.in_paid_time=t3.paid_bit
LEFT JOIN 
	#tmp t4 ON s.in_work_time=t4.work_bit
WHERE 
	NOT EXISTS (SELECT activity_id FROM mart.dim_activity d WHERE d.activity_code = s.activity_code and d.datasource_id=1)

---------------------------------------------------------------------------
-- insert from stg_schedule_shif
--remmar detta tillsvidare, verkar bli knas när det finns scheman med activity_code=null
/*
INSERT INTO mart.dim_activity
	(
	activity_code, 
	in_ready_time,
	in_ready_time_name,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,
	datasource_id,
	datasource_update_date,
	business_unit_id
	)
SELECT 
	activity_code			= s.activity_code,
	in_ready_time			= 0,
	in_ready_time_name		= 'Not In Ready Time',
	in_contract_time		= 0,
	in_contract_time_name	= 'Not In Contract Time',
	in_paid_time			= 0,
	in_paid_time_name		= 'Not In Paid Time',
	in_work_time			= 0,
	in_work_time_name		= 'Not In Work Time',
	datasource_id			= 1,
	datasource_update_date	= s.datasource_update_date,
	business_unit_id		= -1 
FROM
	(SELECT activity_code,datasource_update_date=max(datasource_update_date) FROM Stage.stg_schedule GROUP BY activity_code) s
WHERE 
	NOT EXISTS (SELECT activity_id FROM mart.dim_activity d WHERE d.activity_code = s.activity_code and d.datasource_id=1)
*/
GO