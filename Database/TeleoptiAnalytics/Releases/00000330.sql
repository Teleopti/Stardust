/* 
Trunk initiated: 
2011-06-09 
15:11
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Ola Håkansson  
--Date: 2010-06-14  
--Desc: New report 
----------------  

INSERT INTO [mart].[report]
           ([report_id]
           ,[control_collection_id]
           ,[report_group_id]
           ,[url]
           ,[target]
           ,[report_name]
           ,[report_name_resource_key]
           ,[visible]
           ,[rpt_file_name]
           ,[text_id]
           ,[proc_name]
           ,[help_key]
           ,[sub1_name]
           ,[sub1_proc_name]
           ,[sub2_name]
           ,[sub2_proc_name])
     VALUES
           (25
           ,17
           ,1
           ,'~/Selection.aspx?ReportID=25'
           ,'_blank'
           ,'Activity Time per Agent'
           ,'ResReportActivityTimePerAgent'
           ,1
           ,'~/Reports/CCC/report_activity_time_per_agent.rdlc'
           ,100
           ,'mart.report_data_activity_time_per_agent'
           ,'f01_Report_ActivityTimePerAgent.html'
           ,'' ,'' ,'' ,'')
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (330,'7.1.330') 
