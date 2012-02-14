/* 
Trunk initiated: 
2010-09-24 
10:00
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

----------------
--Name: KJ 
--Date: 2010-09-17 
--Desc: Missing foreign keys on table bridge_group_page_person 
----------------
ALTER TABLE mart.bridge_group_page_person ADD CONSTRAINT
           FK_bridge_group_page_person_dim_group_page FOREIGN KEY
           (
           group_page_id
           ) REFERENCES mart.dim_group_page
           (
           group_page_id
           )
ALTER TABLE mart.bridge_group_page_person ADD CONSTRAINT
           FK_bridge_group_page_person_dim_person FOREIGN KEY
           (
           person_id
           ) REFERENCES mart.dim_person
           (
           person_id
           )

----------------  
--Name: Jonas N
--Date: 2010-09-27
--Desc: Adding group page to report selections 
----------------  
ALTER TABLE [mart].[report_control] ALTER COLUMN control_name VARCHAR(50)

INSERT INTO [mart].[report_control] (control_id, control_name, fill_proc_name, attribute_id)
VALUES (29, 'cboGroupPage', 'mart.report_control_group_page_get', null)
INSERT INTO [mart].[report_control] (control_id, control_name, fill_proc_name, attribute_id)
VALUES (30, 'cboGroupPageGroup', 'mart.report_control_group_page_group_get', null)
INSERT INTO [mart].[report_control] (control_id, control_name, fill_proc_name, attribute_id)
VALUES (31, 'cboGroupPageAgent', 'mart.report_control_group_page_agent_get', null)
INSERT INTO [mart].[report_control] (control_id, control_name, fill_proc_name, attribute_id)
VALUES (32, 'twolistGroupPageAgent', 'mart.report_control_twolist_group_page_agent_get', null)


DECLARE @collection_id int, @print_order int, @id int, @date int

SET @collection_id = 19
SET @print_order = 3
SET @id = 188
SET @date = 151
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 21
SET @print_order = 8
SET @id = 191
SET @date = 170
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 22
SET @print_order = 3
SET @id = 194
SET @date = 180
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 18
SET @print_order = 5
SET @id = 197
SET @date = 140
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 8
SET @print_order = 5
SET @id = 200
SET @date = 47
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 32, -99, 'ResAgentColon', null, '@group_page_agent_set', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 10
SET @print_order = 2
SET @id = 203
SET @date = 64
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @id, @id+1, null)
UPDATE mart.report_control_collection SET depend_of2 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 12
SET @print_order = 6
SET @id = 206
SET @date = 85
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 13
SET @print_order = 4
SET @id = 209
SET @date = 93
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 14
SET @print_order = 5
SET @id = 212
SET @date = 102
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 17
SET @print_order = 5
SET @id = 215
SET @date = 130
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 15
SET @print_order = 3
SET @id = 218
SET @date = 113
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 16
SET @print_order = 3
SET @id = 221
SET @date = 121
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+2, @collection_id, @print_order+3, 31, -2, 'ResAgentColon', null, '@group_page_agent_id', @date, @date+1, @id, @id+1)
UPDATE mart.report_control_collection SET depend_of3 = @id WHERE control_id = 3 AND collection_id = @collection_id

SET @collection_id = 20
SET @print_order = 4
SET @id = 224
SET @date = 158
UPDATE mart.report_control_collection SET print_order = print_order + 3 WHERE collection_id = @collection_id AND print_order > @print_order
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id, @collection_id, @print_order+1, 29, -2, 'ResGroupPageColon', null, '@group_page_code', null, null, null, null)
INSERT INTO mart.report_control_collection (control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4)
           VALUES (@id+1, @collection_id, @print_order+2, 30, -2, 'ResGroupPageGroupColon', null, '@group_page_group_id', @id, null, null, null)
UPDATE mart.report_control_collection SET depend_of2 = @id WHERE control_id = 3 AND collection_id = @collection_id
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (296,'7.1.296') 
