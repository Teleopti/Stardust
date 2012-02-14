IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Select]
GO


----------------------------------------------------------------------------
-- Select a single record from Heartbeat
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Heartbeat_Select]
	@SubscriberId uniqueidentifier
AS

SELECT	HeartbeatId,
	SubscriberId,
	ProcessId,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Heartbeat
WHERE 	SubscriberId = @SubscriberId
GO

