/* 
Trunk initiated: 
2009-12-14 
09:00 (approx.)
By: TOPTINET\andersf
*/ 
----------------  
--Name: David Jonsson
--Date: 2009-12-14
--Desc: Item #8928
--Desc: Add datasource_id (log object) to excuded queue
----------------  

--save data
CREATE TABLE #exclude (queue_original_id int NOT NULL)

INSERT INTO #exclude 
SELECT queue_original_id FROM [mart].[sys_queue_exclude]

--drop table
DROP TABLE [mart].[sys_queue_exclude]

--Re-create table
--1) re-factor name
--2) add column
CREATE TABLE [mart].[dim_queue_excluded](
	[queue_original_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
 CONSTRAINT [PK_dim_queue_exclude] PRIMARY KEY CLUSTERED 
(
	[queue_original_id] ASC,
	[datasource_id] ASC
)
) ON [MART]
GO

--Get all queues back with a cross join to add all existing datasoruce_ids
INSERT INTO [mart].[dim_queue_excluded]
SELECT eq.queue_original_id, ds.datasource_id
FROM mart.sys_datasource ds
CROSS JOIN  #exclude eq
WHERE ds.datasource_id > 1

--Drop temp table
DROP TABLE #exclude
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (184,'7.1.184') 
