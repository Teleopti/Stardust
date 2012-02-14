/****** Object:  StoredProcedure [msg].[sp_Distinct_Heartbeats]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Distinct_Heartbeats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Distinct_Heartbeats]
GO


CREATE PROCEDURE [msg].[sp_Distinct_Heartbeats]
AS
BEGIN
	SELECT '00000000-0000-0000-0000-000000000000' AS HeartbeatId, SubscriberId, ProcessId, ChangedBy, GETDATE() AS ChangedDateTime FROM msg.Heartbeat 
	GROUP BY SubscriberId, ProcessId, ChangedBy
END

GO

