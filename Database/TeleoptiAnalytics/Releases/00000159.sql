/* 
Trunk initiated: 
2009-10-07 
13:05
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: AF & DJ
--Date: 2009-10-07
--Desc: Speed up MsgBroker  
---------------- 
--Temporay added at some customers!! IF EXISTS => Drop
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[msg].[Heartbeat]') AND name = N'IX_Heartbeat_SubscriberId')
DROP INDEX [IX_Heartbeat_SubscriberId] ON [msg].[Heartbeat]
GO

--Re-add
CREATE NONCLUSTERED INDEX [IX_Heartbeat_SubscriberId] ON [msg].[Heartbeat] 
(
 [SubscriberId] ASC
)
GO

EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (159,'7.0.159') 
