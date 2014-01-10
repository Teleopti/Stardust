IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[stage].[v_stg_state_group]'))
DROP VIEW [stage].[v_stg_state_group]
GO

CREATE View [stage].[v_stg_state_group]
AS
SELECT DISTINCT
	s.state_group_code as 'state_group_code',
	s.state_group_name as 'state_group_name',
	1 as 'datasource_id',
	p.business_unit_id as 'business_unit_id'
FROM mart.dim_person p
INNER JOIN stage.stg_agent_state s
	ON p.person_code = s.person_code
WHERE p.person_id <> -1

GO