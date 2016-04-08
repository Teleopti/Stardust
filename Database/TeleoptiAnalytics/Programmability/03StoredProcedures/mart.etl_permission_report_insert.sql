IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_permission_report_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_permission_report_insert]
GO

-- =============================================
-- Description:	Inserts a permission for a user
-- =============================================
CREATE PROCEDURE [mart].[etl_permission_report_insert]
@person_code uniqueidentifier,
@team_code uniqueidentifier,
@my_own bit,
@business_unit_code uniqueidentifier,
@report_id uniqueidentifier
AS
BEGIN

  declare @team_id int = (select team_id from mart.dim_team
  where team_code = @team_code)

  declare @business_unit_id int = (select business_unit_id from mart.dim_business_unit
  where business_unit_code = @business_unit_code)

  insert into mart.permission_report
	([person_code]
	,[team_id]
	,[my_own]
	,[business_unit_id]
	,[datasource_id]
	,[datasource_update_date]
	,[ReportId])
  values (@person_code
	  ,@team_id
	  ,@my_own
	  ,@business_unit_id
	  ,1
	  ,GETUTCDATE()
	  ,@report_id)
END

GO



