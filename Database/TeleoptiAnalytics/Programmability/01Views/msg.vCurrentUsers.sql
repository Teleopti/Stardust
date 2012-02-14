IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[Msg].[vCurrentUsers]'))
DROP VIEW [Msg].[vCurrentUsers]
GO



CREATE VIEW [msg].[vCurrentUsers]
AS
SELECT DISTINCT ProcessId, ChangedBy as UserName 
FROM Msg.HEARTBEAT 

GO

