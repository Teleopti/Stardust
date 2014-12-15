----------------  
--Name: KJ
--Date: 2014-12-12 
--Desc: Remove timezone as a parameter from Agent Skill report
----------------  
DELETE FROM mart.report_control_collection WHERE collection_id=46 and control_collection_id=495
GO