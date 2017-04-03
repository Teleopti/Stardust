IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_acd_login_person_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_acd_login_person_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads bridge, that connects acd_login with persons.
------------------------------------------------
-- Update date Log
-- 2009-02-11 New mart schema KJ
-- 2009-02-10 Bug fix for multi BU KJ
-- 2009-02-09 Stage table moved to mart db, removed view KJ
-- 2008-12-15 Bring all persons and all acd_logins in bridge table KJ
-- 2008-12-04 fixes for multi BU load KJ
-- 2008-08-13 Added team_id KJ
-- 2009-04-27 Change dateformat on min/max date DJ
-- 2010-10-22 Cover the case when dim person is not loaded correctly (due to missing dim date)
-- 2010-11-08 Changed to use person_period_code in join instead of dates RK/TB
-- =============================================
--exec mart.etl_bridge_acd_login_person_load '77F2862D-7E84-470A-A2FC-9B6B00D7B265'
CREATE PROCEDURE [mart].[etl_bridge_acd_login_person_load] 
@business_unit_code uniqueidentifier	
AS

DECLARE @maxdate as smalldatetime
SELECT @maxdate=CAST('20591231' as smalldatetime)

/*Get business unit id*/
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

/*Delete data for given business unit*/
DELETE FROM mart.bridge_acd_login_person 
WHERE business_unit_id = @business_unit_id
	OR business_unit_id = -1

-- Insert new queues
INSERT INTO mart.bridge_acd_login_person
	( 
	acd_login_id, 
	person_id, 
	team_id,
	business_unit_id,
	datasource_id, 
	insert_date, 
	update_date, 
	datasource_update_date
	)
SELECT 
	acd_login_id	= isnull(da.acd_login_id,-1),
	person_id		= isnull(p.person_id,-1),
	team_id			= isnull(p.team_id,-1),
	business_unit_id = isnull(p.business_unit_id,-1),
	datasource_id	= isnull(a.datasource_id,-1), 
	insert_date		= getdate(), 
	update_date		= getdate(), 
	datasource_update_date	= isnull(a.datasource_update_date,@maxdate)
FROM
	Stage.stg_acd_login_person a
INNER JOIN
	mart.dim_person p WITH (NOLOCK)
ON
	a.person_period_code = p.person_period_code
FULL OUTER JOIN
	mart.dim_acd_login	da
ON
	a.acd_login_code			= da.acd_login_original_id	AND
	a.log_object_datasource_id	= da.datasource_id
INNER JOIN 
	mart.dim_business_unit bu ON bu.business_unit_id=p.business_unit_id
	AND bu.business_unit_id=@business_unit_id



INSERT INTO mart.bridge_acd_login_person
	( 
	acd_login_id, 
	person_id, 
	team_id,
	business_unit_id,
	datasource_id, 
	insert_date, 
	update_date, 
	datasource_update_date
	)
SELECT 
	acd_login_id	= da.acd_login_id,
	person_id		= -1,
	team_id			= -1,
	business_unit_id = -1,
	datasource_id	= -1, 
	insert_date		= getdate(), 
	update_date		= getdate(), 
	datasource_update_date	= @maxdate
FROM
	mart.dim_acd_login	da
WHERE acd_login_id NOT IN (SELECT acd_login_id FROM mart.bridge_acd_login_person)


GO
