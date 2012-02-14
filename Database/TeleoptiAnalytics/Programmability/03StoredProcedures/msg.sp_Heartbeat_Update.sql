IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Heartbeat_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Heartbeat_Update]
GO




----------------------------------------------------------------------------
-- Update a single record in Heartbeat
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Heartbeat_Update]
	@HeartbeatId uniqueidentifier,
	@ProcessId int,
	@ChangedBy nvarchar(100),
	@ChangedDateTime datetime
AS

UPDATE	Msg.Heartbeat
SET	ProcessId = @ProcessId,
	ChangedBy = @ChangedBy,
	ChangedDateTime = @ChangedDateTime
WHERE 	HeartbeatId = @HeartbeatId



GO

