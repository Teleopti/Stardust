----------------  
--Name: JN
--Date: 2015-01-22 
--Desc: Delete ETL special toggles since we now can use the web to access toggles instead.
----------------  

DELETE FROM mart.sys_configuration
WHERE [key] in ('ETL_SpeedUpETL_30791', 'PBI30787OnlyLatestQueueAgentStatistics')