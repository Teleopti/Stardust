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
-- =============================================
--exec mart.etl_permission_report_load @business_unit_code='556E598E-DEC7-44CE-BA6C-A01A01284810',@isFirstBusinessUnit=1,@isLastBusinessUnit=0
CREATE PROCEDURE [mart].[etl_permission_report_load]
@business_unit_code uniqueidentifier,
@isFirstBusinessUnit bit,
@isLastBusinessUnit bit
AS
DECLARE @is_active char(1)
DECLARE @non_active char(1)
SELECT @is_active = is_active FROM [mart].[permission_report_active]

IF @is_active = 'A'
	SET @non_active = 'B'
ELSE
	SET @non_active = 'A'
	
--SAVE NEW PERMISSIONS
BEGIN TRY
	INSERT INTO mart.v_permission_report
		(ReportId, person_code, team_id, my_own, business_unit_id, datasource_id, datasource_update_date, table_name)
	SELECT	ReportId				= pr.ReportId,
			person_code				= pr.person_code,
			team_id					= dt.team_id,
			my_own					= pr.my_own,
			business_unit_id		= bu.business_unit_id,
			datasource_id			= pr.datasource_id, 
			datasource_update_date	= pr.datasource_update_date,
			table_name				= @non_active
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
END TRY
BEGIN CATCH
	DECLARE @ErrorMessage NVARCHAR(4000);
	DECLARE @ErrorSeverity INT;
	DECLARE @ErrorState INT;

	SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(),@ErrorState = ERROR_STATE();
	
	--Try save the previous data from the active permission table
	IF @non_active = 'A'
		insert into [mart].[permission_report_A] (person_code, team_id, my_own, business_unit_id, datasource_id, ReportId, datasource_update_date, table_name)
		select person_code, team_id, my_own, business_unit_id, datasource_id, ReportId, datasource_update_date, 'A' from [mart].[permission_report_B]
	ELSE
		insert into [mart].[permission_report_B] (person_code, team_id, my_own, business_unit_id, datasource_id, ReportId, datasource_update_date, table_name)
		select person_code, team_id, my_own, business_unit_id, datasource_id, ReportId, datasource_update_date, 'B' from [mart].[permission_report_A]

	--return error to ETL
	IF @ErrorState < 1
	SET @ErrorState = 1

	SET @ErrorMessage = 'Procedure [mart].[etl_permission_report_load] failed: ' + @ErrorMessage
	RAISERROR (@ErrorMessage,@ErrorSeverity,@ErrorState);
	
END CATCH

GO

