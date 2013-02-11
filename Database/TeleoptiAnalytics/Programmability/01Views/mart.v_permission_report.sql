IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_permission_report]'))
DROP VIEW [mart].[v_permission_report]
GO

CREATE VIEW [mart].[v_permission_report]
WITH SCHEMABINDING
AS
SELECT [person_code]
      ,[team_id]
      ,[my_own]
      ,[business_unit_id]
      ,[datasource_id]
      ,[ReportId]
	  ,[datasource_update_date]
      ,[table_name]
  FROM [mart].[permission_report_A] WITH (NOLOCK)
UNION ALL
SELECT [person_code]
      ,[team_id]
      ,[my_own]
      ,[business_unit_id]
      ,[datasource_id]
      ,[ReportId]
      ,[datasource_update_date]
      ,[table_name]
  FROM [mart].[permission_report_B] WITH (NOLOCK)
GO

--Added for backward compability and not to break CRA
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[permission_report]'))
DROP VIEW [mart].[permission_report]
GO

CREATE VIEW [mart].[permission_report]
AS
SELECT
	person_code,
	team_id,
	my_own,
	business_unit_id,
	datasource_id,
	getdate() insert_date, --dummy column for non-breaking change
	getdate() update_date, --dummy column for non-breaking change
	datasource_update_date,
	ReportId
FROM mart.v_permission_report perm
INNER JOIN [mart].[permission_report_active] active ON perm.table_name = active.is_active

GO