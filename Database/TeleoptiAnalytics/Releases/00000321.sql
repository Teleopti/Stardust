/* 
Trunk initiated: 
2011-03-24 
08:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Mattias E
--Date: 2011-03-30  
--Desc: Add report Agent Queue Metrics  
----------------  
------------
--Add report
------------
insert into mart.report
select 24, 24, 1, '~/Selection.aspx?ReportID=24','_blank','Agent Queue Metrics','ResReportAgentQueueMetrics',1,'~/Reports/CCC/report_agent_queue_metrics.rdlc',1000,'mart.report_data_agent_queue_metrics','f01_Report_AgentQueueMetrics.html','','','',''

-------------
--Add selection params
-------------
INSERT INTO [mart].[report_control_collection]
SELECT '239','24','1','1','12:00','ResDateFromColon',NULL,'@date_from',NULL,NULL,NULL,NULL UNION ALL
SELECT '240','24','2','2','12:00','ResDateToColon',NULL,'@date_to','239',NULL,NULL,NULL UNION ALL
SELECT '241','24','3','12','0','ResIntervalFromColon','1','@interval_from',NULL,NULL,NULL,NULL UNION ALL
SELECT '242','24','4','13','-99','ResIntervalToColon','2','@interval_to',NULL,NULL,NULL,NULL UNION ALL
SELECT '243','24','5','29','-2','ResGroupPageColon',NULL,'@group_page_code',NULL,NULL,NULL,NULL UNION ALL
SELECT '247','24','6','30','-2','ResGroupPageGroupColon',NULL,'@group_page_group_id','243',NULL,NULL,NULL UNION ALL
SELECT '248','24','7','32','-2','ResAgentColon',NULL,'@group_page_agent_set','239','240','243','247' UNION ALL
SELECT '244','24','8','3','-2','ResSiteNameColon',NULL,'@site_id','239','240','243',NULL UNION ALL
SELECT '245','24','9','4','-2','ResTeamNameColon',NULL,'@team_id','239','240','244',NULL UNION ALL
SELECT '246','24','10','18','-2','ResAgentsColon',NULL,'@agent_set','239','240','244','245' UNION ALL
SELECT '249','24','11','22','-1','ResTimeZoneColon',NULL,'@time_zone_id',NULL,NULL,NULL,NULL 
GO 
 

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (321,'7.1.321') 
