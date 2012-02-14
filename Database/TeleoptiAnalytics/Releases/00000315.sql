/* 
Trunk initiated: 
2011-01-27 
14:22
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2011-01-30
--Desc: Cut some time off Queue and Agent ETL-load
--Note: added as IF EXISTS in case we need to add them outside release 7.1.315
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_agent]') AND name = N'IX_datasource_localdate_date_interval_acd')
	CREATE NONCLUSTERED INDEX IX_datasource_localdate_date_interval_acd
	ON [mart].[fact_agent] ([datasource_id],[local_date_id])
	INCLUDE ([date_id],[interval_id],[acd_login_id])
	ON [MART]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_agent_queue]') AND name = N'IX_datasource_localdate_date_interval_acd')
	CREATE NONCLUSTERED INDEX IX_datasource_localdate_date_interval_acd
	ON [mart].[fact_agent_queue] ([datasource_id],[local_date_id])
	INCLUDE ([date_id],[interval_id],[queue_id],[acd_login_id])
	ON [MART]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_queue]') AND name = N'IX_datasource_localdate_date_interval_queue')
	CREATE NONCLUSTERED INDEX IX_datasource_localdate_date_interval_queue
	ON [mart].[fact_queue] ([datasource_id],[local_date_id])
	INCLUDE ([date_id],[interval_id],[queue_id])
	ON [MART]
GO 

----------------  
--Name: David Jonsson
--Date: 2011-01-30
--Desc: Cut some time off Queue and Agent ETL-load,indexes added at live customer
--Note: added as IF EXISTS since this is already release outside 7.1.315
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND name = N'IX_datasource')
	CREATE NONCLUSTERED INDEX [IX_datasource]
	ON [mart].[dim_acd_login] ([datasource_id])
	INCLUDE ([acd_login_id],[acd_login_agg_id])
	 ON [MART]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND name = N'IX_acd_datasource')
	CREATE NONCLUSTERED INDEX [IX_acd_datasource]
	ON [mart].[dim_acd_login] ([acd_login_agg_id],[datasource_id])
	INCLUDE ([acd_login_id])
	 ON [MART]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_queue]') AND name = N'IX_aggId_datasource')
	CREATE NONCLUSTERED INDEX [IX_aggId_datasource]
	ON [mart].[dim_queue] ([queue_agg_id],[datasource_id])
	INCLUDE ([queue_id])
	 ON [MART]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_queue]') AND name = N'IX_datasource')
	CREATE NONCLUSTERED INDEX [IX_datasource]
	ON [mart].[dim_queue] ([datasource_id])
	INCLUDE ([queue_id],[queue_agg_id])
	 ON [MART]
GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (315,'7.1.315') 
