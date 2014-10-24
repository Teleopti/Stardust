----------------  
--Name: Karin
--Date: 2014-10-24
--Desc: PBI #30787 - Change of detail type so that it matches Agg db
----------------
UPDATE [mart].[sys_datasource_detail_type]
SET [detail_desc]='Queue'
WHERE [detail_id]=1
GO
UPDATE [mart].[sys_datasource_detail_type]
SET [detail_desc]='Agent'
WHERE [detail_id]=2
GO
