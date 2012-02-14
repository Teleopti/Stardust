IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Heartbeat_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Heartbeat_Delete]
GO




----------------------------------------------------------------------------
-- Delete a single record from Heartbeat
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Heartbeat_Delete]
	@HeartbeatId uniqueidentifier
AS

DELETE	Msg.Heartbeat
WHERE 	HeartbeatId = @HeartbeatId



GO

