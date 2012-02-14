----------------  
--Name: Jonas n + David j
--Date: 2012-01-04  + 2012-01-09
--Desc: Add FK´s to the three statistics tables and set to not nullable
----------------
-- Rename current FKs to sufix to indicate UTC
EXEC sp_rename N'mart.FK_fact_agent_dim_date1', N'FK_fact_agent_dim_date_UTC', N'OBJECT'
EXEC sp_rename N'mart.FK_fact_agent_queue_dim_interval', N'FK_fact_agent_queue_dim_interval_UTC', N'OBJECT'
EXEC sp_rename N'mart.FK_fact_agent_dim_interval', N'FK_fact_agent_dim_interval_UTC', N'OBJECT'
EXEC sp_rename N'mart.FK_fact_queue_dim_date', N'FK_fact_queue_dim_date_UTC', N'OBJECT'
EXEC sp_rename N'mart.FK_fact_queue_dim_interval', N'FK_fact_queue_dim_interval_UTC', N'OBJECT'
EXEC sp_rename N'mart.FK_fact_agent_queue_dim_date', N'FK_fact_agent_queue_dim_date_UTC', N'OBJECT'

-- Create new FK´s
ALTER TABLE mart.fact_agent ADD CONSTRAINT FK_fact_agent_dim_date_Local FOREIGN KEY(local_date_id)
REFERENCES mart.dim_date (date_id)
GO
ALTER TABLE mart.fact_agent ADD CONSTRAINT FK_fact_agent_dim_interval_Local FOREIGN KEY(local_interval_id)
REFERENCES mart.dim_interval (interval_id)
GO

ALTER TABLE mart.fact_agent_queue ADD CONSTRAINT FK_fact_agent_queue_dim_date_Local FOREIGN KEY(local_date_id)
REFERENCES mart.dim_date (date_id)
GO
ALTER TABLE mart.fact_agent_queue ADD CONSTRAINT FK_fact_agent_queue_dim_interval_Local FOREIGN KEY(local_interval_id)
REFERENCES mart.dim_interval (interval_id)
GO


ALTER TABLE mart.fact_queue ADD CONSTRAINT FK_fact_queue_dim_date_Local FOREIGN KEY(local_date_id)
REFERENCES mart.dim_date (date_id)
GO
ALTER TABLE mart.fact_queue ADD CONSTRAINT FK_fact_queue_dim_interval_Local FOREIGN KEY(local_interval_id)
REFERENCES mart.dim_interval (interval_id)
GO


-- Set local_date_id and local_interval_id to not nullable
UPDATE mart.fact_agent SET local_date_id = -1 WHERE local_date_id IS NULL
UPDATE mart.fact_agent SET local_interval_id = 0 WHERE local_interval_id IS NULL


-- temporary drop index
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_agent]') AND name = N'IX_datasource_localdate_date_interval_acd')
DROP INDEX [IX_datasource_localdate_date_interval_acd] ON [mart].[fact_agent]

ALTER TABLE mart.fact_agent ALTER COLUMN local_date_id INT NOT NULL
ALTER TABLE mart.fact_agent ALTER COLUMN local_interval_id SMALLINT NOT NULL
GO
-- re-add index
CREATE NONCLUSTERED INDEX [IX_datasource_localdate_date_interval_acd] ON [mart].[fact_agent] 
(
	[datasource_id] ASC,
	[local_date_id] ASC
)
INCLUDE ( [date_id],
[interval_id],
[acd_login_id]
)
GO

UPDATE mart.fact_agent_queue SET local_date_id = -1 WHERE local_date_id IS NULL
UPDATE mart.fact_agent_queue SET local_interval_id = 0 WHERE local_interval_id IS NULL

-- temporary drop index
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_agent_queue]') AND name = N'IX_datasource_localdate_date_interval_acd')
DROP INDEX [IX_datasource_localdate_date_interval_acd] ON [mart].[fact_agent_queue]
GO

ALTER TABLE mart.fact_agent_queue ALTER COLUMN local_date_id INT NOT NULL
ALTER TABLE mart.fact_agent_queue ALTER COLUMN local_interval_id SMALLINT NOT NULL
GO

--re-add index
CREATE NONCLUSTERED INDEX [IX_datasource_localdate_date_interval_acd] ON [mart].[fact_agent_queue] 
(
	[datasource_id] ASC,
	[local_date_id] ASC
)
INCLUDE ( [date_id],
[interval_id],
[queue_id],
[acd_login_id]
)
GO


UPDATE mart.fact_queue SET local_date_id = -1 WHERE local_date_id IS NULL
UPDATE mart.fact_queue SET local_interval_id = 0 WHERE local_interval_id IS NULL
GO

-- temporary drop index
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_queue]') AND name = N'IX_datasource_localdate_date_interval_queue')
DROP INDEX [IX_datasource_localdate_date_interval_queue] ON [mart].[fact_queue]
GO

ALTER TABLE mart.fact_queue ALTER COLUMN local_date_id INT NOT NULL
ALTER TABLE mart.fact_queue ALTER COLUMN local_interval_id SMALLINT NOT NULL
GO

--re-add index
CREATE NONCLUSTERED INDEX [IX_datasource_localdate_date_interval_queue] ON [mart].[fact_queue] 
(
	[datasource_id] ASC,
	[local_date_id] ASC
)
INCLUDE ( [date_id],
[interval_id],
[queue_id]
)
GO
----------------  
--Name: Jonas n
--Date: 2012-01-09  
--Desc: Correct a earlier incorrect update script regarding report selection dependencies together with multi team selection.
----------------
UPDATE mart.report_control_collection SET depend_of4 = 256, depend_of3 = 253 WHERE control_collection_id = 257
UPDATE mart.report_control_collection SET depend_of4 = 267, depend_of3 = 264 WHERE control_collection_id = 268
UPDATE mart.report_control_collection SET depend_of4 = 283, depend_of3 = 280 WHERE control_collection_id = 284
UPDATE mart.report_control_collection SET depend_of4 = 293, depend_of3 = 290 WHERE control_collection_id = 294
UPDATE mart.report_control_collection SET depend_of4 = 305, depend_of3 = 302 WHERE control_collection_id = 306
UPDATE mart.report_control_collection SET depend_of4 = 319, depend_of3 = 316 WHERE control_collection_id = 320
UPDATE mart.report_control_collection SET depend_of4 = 330, depend_of3 = 327 WHERE control_collection_id = 331
UPDATE mart.report_control_collection SET depend_of3 = 341, depend_of2 = 339 WHERE control_collection_id = 342

----------------  
--Name: Anders F
--Date: 2012-01-18
--Desc: In some rare cases MB was already messed up so let's fix that during upgrade just in case
----------------
if (select COUNT(*) from msg.event) > 1500
	truncate table msg.event

if (select COUNT(*) from msg.Filter) > 1500
	truncate table msg.filter

if (select COUNT(*) from msg.Heartbeat) > 1500
	truncate table msg.heartbeat

if (select COUNT(*) from msg.Log) > 1500
	truncate table msg.log

if (select COUNT(*) from msg.Pending) > 1500
	truncate table msg.pending

if (select COUNT(*) from msg.Receipt) > 1500
	truncate table msg.receipt
	
if (select COUNT(*) from msg.Subscriber) > 1500
	truncate table msg.subscriber

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (350,'7.1.350') 
