IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Heartbeats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Heartbeats]
GO



CREATE PROCEDURE [Msg].[sp_Heartbeats]
AS
SELECT HeartbeatId, ProcessId, ChangedBy, ChangedDateTime 
FROM Msg.Heartbeat 
ORDER BY ChangedDateTime DESC


GO

