IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_bridge_agent_skill_tabular]'))
DROP VIEW [mart].[v_bridge_agent_skill_tabular]
GO
CREATE VIEW [mart].[v_bridge_agent_skill_tabular]
AS
SELECT        mart.dim_person.person_id, mart.bridge_skillset_skill.skill_id
FROM            mart.bridge_skillset_skill INNER JOIN
                         mart.dim_person ON mart.bridge_skillset_skill.skillset_id = mart.dim_person.skillset_id
GO
