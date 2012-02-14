/* 
Trunk initiated: 
2011-01-13 
10:36
By: TOPTINET\davidj 
On INTEGRATION 
*/ 
----------------  
--Name: Mattias E  
--Date: 2011-01-25  
--Desc: Update parameters @agent_id to @agent_code and @group_page_agent_id to @group_page_agent_code
--		in the report control collection
----------------  

update mart.report_control_collection
set param_name = '@agent_code'
from mart.report_control_collection rcc
inner join mart.report r
on rcc.collection_id = r.control_collection_id
where param_name = '@agent_id'
and (report_id = 1
	OR report_id = 2
	OR report_id = 4
	OR report_id = 15
	OR report_id = 16
	OR report_id = 17
	OR report_id = 18
	OR report_id = 19
	OR report_id = 20
	OR report_id = 22)

update mart.report_control_collection
set param_name = '@group_page_agent_code'
from mart.report_control_collection rcc
inner join mart.report r
on rcc.collection_id = r.control_collection_id
where param_name = '@group_page_agent_id'
and (report_id = 1
	OR report_id = 2
	OR report_id = 4
	OR report_id = 15
	OR report_id = 16
	OR report_id = 17
	OR report_id = 18
	OR report_id = 19
	OR report_id = 20
	OR report_id = 22)

update mart.report_control_collection
set param_name = '@agent_person_code'
from mart.report_control_collection rcc
inner join mart.report r
on rcc.collection_id = r.control_collection_id
where param_name = '@agent_person_id'
and report_id = 13

update mart.report_control_collection
set param_name = '@group_page_agent_code'
from mart.report_control_collection rcc
inner join mart.report r
on rcc.collection_id = r.control_collection_id
where param_name = '@group_page_agent_id'
and report_id = 13

----------------  
--Name: David J
--Date: 2011-01-27
--Desc: Network runner table
----------------  
--drop obsolete table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkRunner]') AND type in (N'U'))
	DROP TABLE [dbo].[NetworkRunner];
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (314,'7.1.314') 
