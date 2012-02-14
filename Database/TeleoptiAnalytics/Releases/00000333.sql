----------------  
--Name: David J
--Date: 2011-09-05
--Desc: Re-order clustered key on table msg.Filter
----------------  
--Drop existing PK
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[msg].[Filter]') AND name = N'PK_Filter')
ALTER TABLE [msg].[Filter] DROP CONSTRAINT [PK_Filter]
GO

--Add better clustered index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[msg].[Filter]') AND name = N'CIX_SubscriberId')
CREATE CLUSTERED INDEX [CIX_SubscriberId] ON [msg].[Filter] ([SubscriberId])
GO

--re-add PK
ALTER TABLE msg.Filter ADD CONSTRAINT
	PK_Filter PRIMARY KEY NONCLUSTERED 
	(
	FilterId
	)
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (333,'7.1.333') 
