IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_bridge_acd_login_person_date]'))
DROP VIEW [mart].[v_bridge_acd_login_person_date]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2010-09-13
-- Description:	Used in the cube to get agent data in correlation to schedule data
------------------------------------------------
-- Updates:		
-- =============================================

CREATE VIEW mart.v_bridge_acd_login_person_date

AS
SELECT DISTINCT d .date_id, fa.acd_login_id, b.person_id, p.valid_from_date_id, p.valid_to_date_id, p.team_id, p.business_unit_id
FROM         mart.dim_date AS d INNER JOIN
                      mart.fact_agent AS fa ON fa.date_id = d .date_id INNER JOIN
                      mart.bridge_acd_login_person AS b ON b.acd_login_id = fa.acd_login_id INNER JOIN
                      mart.dim_person AS p ON p.person_id = b.person_id AND d .date_id > p.valid_from_date_id
WHERE     (d .date_id <= p.valid_to_date_id) OR
                      (p.valid_to_date_id = - 2)
UNION
SELECT DISTINCT d .date_id, b.acd_login_id, fs.person_id, p.valid_from_date_id, p.valid_to_date_id, p.team_id, p.business_unit_id
FROM         mart.dim_date AS d INNER JOIN
                      mart.fact_schedule AS fs ON fs.schedule_date_id = d .date_id INNER JOIN
                      mart.bridge_acd_login_person AS b ON b.person_id = fs.person_id INNER JOIN
                      mart.dim_person AS p ON p.person_id = b.person_id AND d .date_id > p.valid_from_date_id
WHERE     (d .date_id <= p.valid_to_date_id) OR
                      (p.valid_to_date_id = - 2)
