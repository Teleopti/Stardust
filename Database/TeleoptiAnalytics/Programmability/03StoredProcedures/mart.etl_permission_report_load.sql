IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_permission_report_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_permission_report_load]
GO


-- =============================================
-- Author:		<KJ>
-- Create date: <2008-06-27>
-- Description:	<Loads permission data från stage db>
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-06-30 Added join to mart.report on INSERT KJ
-- 2011-04-07 Added input parameter for business uint ME
-- 2013-02-07 #22206 Use View on top of two permission tables DJ
-- 2013-05-06 Back to the "old" way beacuse we have changed inside the ETL-Tool. Ola
-- =============================================
--exec mart.etl_permission_report_load @business_unit_code='556E598E-DEC7-44CE-BA6C-A01A01284810'
CREATE PROCEDURE [mart].[etl_permission_report_load]
@business_unit_code uniqueidentifier
AS

--DELETE OLD PERMISSIONS
DELETE FROM mart.permission_report
WHERE business_unit_id = 
	(
		SELECT DISTINCT
			business_unit_id
		FROM mart.dim_business_unit
		WHERE business_unit_code = @business_unit_code
	)

--SAVE NEW PERMISSIONS
INSERT mart.permission_report
	(ReportId, person_code, team_id, my_own, business_unit_id, datasource_id, datasource_update_date)
SELECT	ReportId				= pr.ReportId,
		person_code				= pr.person_code,
		team_id					= dt.team_id,
		my_own					= pr.my_own,
		business_unit_id		= bu.business_unit_id,
		datasource_id			= pr.datasource_id, 
		datasource_update_date	= pr.datasource_update_date
FROM 
	Stage.stg_permission_report pr
INNER JOIN 
	mart.dim_team dt 
ON 
	dt.team_code = pr.team_id
INNER JOIN
	mart.dim_business_unit bu
ON
	pr.business_unit_code = bu.business_unit_code
INNER JOIN 
	mart.v_report r
ON
	pr.ReportId= r.Id
ORDER BY pr.ReportId, pr.person_code

GO

