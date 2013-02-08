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