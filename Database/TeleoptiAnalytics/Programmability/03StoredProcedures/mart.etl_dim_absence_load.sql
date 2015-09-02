/****** Object:  StoredProcedure [mart].[etl_dim_absence_load]    Script Date: 12/01/2008 14:56:21 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_absence_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_absence_load]
GO
/****** Object:  StoredProcedure [mart].[etl_dim_absence_load]    Script Date: 12/01/2008 14:56:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-16
-- Description:	Loads absence from stg_absence to dim_absence.
-- Update date: 2012-11-21
-- 2012-11-21 Added new column display_color_html KJ
-- 2009-02-11 New Mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2008-12-01 Added new fields in_contract_time,in_contract_time_name,in_paid_time,in_pad_time_name,in_work_time,in_work_time_name KJ
-- 2008-09-26 Changed absences name and display_color loaded from stg_schedule. Can not be null in absence_name field KJ

-- =============================================
CREATE PROCEDURE [mart].[etl_dim_absence_load] 
@business_unit_code uniqueidentifier	
WITH EXECUTE AS OWNER
AS
--------------------------------------------------------------------------
-- Not Defined Absence
SET IDENTITY_INSERT mart.dim_absence ON

INSERT INTO mart.dim_absence
	(
	absence_id,
	absence_code,
	absence_name, 
	display_color,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,
	is_deleted,
	display_color_html,
	absence_shortname
	)
SELECT 
	absence_id				= -1, 
	absence_code			= '00000000-0000-0000-0000-000000000000',
	absence_name			= 'Not Defined', 
	display_color			= -1,
	in_contract_time		= 0,
	in_contract_time_name	= 'Not In Contract Time',
	in_paid_time			= 0,
	in_paid_time_name		= 'Not In Paid Time',
	in_work_time			= 0,
	in_work_time_name		= 'Not In Work Time',
	is_deleted				= 0,
	display_color_html		= '#FFFFFF',
	absence_shortname		= 'Not Defined'
WHERE NOT EXISTS (SELECT * FROM mart.dim_absence where absence_id = -1)

SET IDENTITY_INSERT mart.dim_absence OFF

CREATE TABLE #tmp(contract_bit bit, contract_description nvarchar(50),paid_bit bit, paid_description nvarchar(50),work_bit bit, work_description nvarchar(50))
INSERT #tmp(contract_bit,contract_description,paid_bit,paid_description,work_bit,work_description) SELECT 0,'Not In Contract Time',0,'Not In Paid Time',0,'Not In Work Time'
INSERT #tmp(contract_bit,contract_description,paid_bit,paid_description,work_bit,work_description) SELECT 1,'In Contract Time',1,'In Paid Time',1,'In Work Time'

---------------------------------------------------------------------------
-- update changes on absences
UPDATE mart.dim_absence
SET 
	absence_name		= s.absence_name,
	display_color		= s.display_color,
	in_contract_time	= s.in_contract_time,
	in_contract_time_name	= t1.contract_description,
	in_paid_time		= s.in_paid_time,
	in_paid_time_name	= t2.paid_description ,
	in_work_time		= s.in_work_time,
	in_work_time_name	= t3.work_description,
	is_deleted			= s.is_deleted,
	display_color_html	= s.display_color_html, 
	absence_shortname	= s.absence_shortname
FROM
	Stage.stg_absence s
LEFT JOIN 
	#tmp t1 ON s.in_contract_time=t1.contract_bit 
LEFT JOIN 
	#tmp  t2 ON s.in_paid_time=t2.paid_bit 
LEFT JOIN 
	#tmp t3 ON s.in_work_time=t3.work_bit
WHERE 
	s.absence_code = mart.dim_absence.absence_code

-- Insert new absences
INSERT INTO mart.dim_absence
	(
	absence_code, 
	absence_name, 
	display_color,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,	
	business_unit_id,
	is_deleted,
	display_color_html,
	absence_shortname
	)
SELECT 
	absence_code		= s.absence_code, 
	absence_name		= s.absence_name, 
	display_color		= s.display_color,
	in_contract_time	= s.in_contract_time,
	in_contract_time_name = t1.contract_description,
	in_paid_time		= s.in_paid_time,
	in_paid_time_name	= t2.paid_description ,
	in_work_time		= s.in_work_time,
	in_work_time_name	= t3.work_description,
	business_unit_id	= bu.business_unit_id,
	is_deleted			= s.is_deleted,
	display_color_html	= s.display_color_html,
	absence_shortname	= s.absence_shortname
FROM
	Stage.stg_absence s	
JOIN
	mart.dim_business_unit bu
ON
	s.business_unit_code =bu.business_unit_code	
LEFT JOIN 
	#tmp t1 ON s.in_contract_time=t1.contract_bit 
LEFT JOIN 
	#tmp  t2 ON s.in_paid_time=t2.paid_bit 
LEFT JOIN 
	#tmp t3 ON s.in_work_time=t3.work_bit
WHERE 
	NOT EXISTS (SELECT absence_id FROM mart.dim_absence d WHERE d.absence_code = s.absence_code and d.datasource_id=1)

/*
---------------------------------------------------------------------------
-- insert from stg_schedule_shif absences not known in Absences(deleted)
-- Kolla om denna verlkigen kan inträffa! OM(?) ETL-koden hämtar alla (även deletade) absences, borde denna kunna tas bort.
INSERT INTO mart.dim_absence
	(
	absence_code, 
	absence_name,
	display_color,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,
	datasource_id,
	datasource_update_date,
	business_unit_id,
	is_deleted,
	display_color_html,
	absence_shortname
	)
SELECT 
	absence_code			= s.absence_code,
	absence_name			= '[Unknown]',
	display_color			= -1,
	in_contract_time		= 0,
	in_contract_time_name	= 'Not In Contract Time',
	in_paid_time			= 0,
	in_paid_time_name		= 'Not In Paid Time',
	in_work_time			= 0,
	in_work_time_name		= 'Not In Work Time',
	datasource_id			= 1,
	datasource_update_date	= s.datasource_update_date,
	business_unit_id		= -1,
	is_deleted				= 1,
	display_color_html		= '#FFFFFF',
	absence_shortname		= 'Not Defined'
FROM
	(SELECT absence_code,datasource_update_date=max(datasource_update_date) FROM Stage.stg_schedule WHERE NOT absence_code IS NULL GROUP BY absence_code) s
WHERE 
	NOT EXISTS (SELECT absence_id FROM mart.dim_absence d WHERE d.absence_code = s.absence_code and d.datasource_id=1)

*/

GO