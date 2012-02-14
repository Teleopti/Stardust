/* 
Trunk initiated: 
2010-03-24 
08:09
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonssno
--Date: 2010-03-24
--Desc: Add index to RTA state table
----------------
--This index is already deliver as patch for some customers => IF NOT EXISTS
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[RTA].[ExternalAgentState]') AND name = N'IXC_LogOn_Timestamp')
CREATE CLUSTERED INDEX [IXC_LogOn_Timestamp] ON [RTA].[ExternalAgentState] 
(
	[LogOn] ASC,
	[TimestampValue] ASC
) ON [PRIMARY]
GO

--But need to be renamed
EXEC sp_rename N'[RTA].[ExternalAgentState].[IXC_LogOn_Timestamp]', N'IXC_ExternalAgentState_LogOn_Timestamp', N'INDEX'

----------------  
--Name: Robin Karlsson
--Date: 2010-03-26
--Desc: Delete configuration parameter for ConnectionString as it isn't used anymore
----------------
DELETE FROM msg.Configuration WHERE ConfigurationType='TeleoptiBrokerService' AND ConfigurationName='ConnectionString'
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (221,'7.1.221') 
