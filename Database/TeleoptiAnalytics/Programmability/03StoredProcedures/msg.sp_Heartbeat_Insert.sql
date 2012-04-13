/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Insert]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Insert]
GO


----------------------------------------------------------------------------
-- Insert a single record into Heartbeat
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Heartbeat_Insert]
	@HeartbeatId uniqueidentifier OUTPUT,
	@SubscriberId uniqueidentifier,
	@ProcessId int,
	@ChangedBy nvarchar(100),
	@ChangedDateTime datetime
AS
BEGIN
--reset input to SQL Server time
SELECT @ChangedDateTime = GETDATE()

IF(@HeartbeatId = '00000000-0000-0000-0000-000000000000')
	BEGIN
		SET @HeartbeatId = newid()
		INSERT Msg.Heartbeat(HeartbeatId, SubscriberId, ProcessId, ChangedBy, ChangedDateTime)
		VALUES (@HeartbeatId, @SubscriberId, @ProcessId, @ChangedBy, @ChangedDateTime)
	END
ELSE
	IF(EXISTS(SELECT 1 FROM Msg.Heartbeat WHERE HeartbeatId = @HeartbeatId))
		BEGIN
				UPDATE	Msg.Heartbeat
				SET	ProcessId = @ProcessId,
					ChangedBy = @ChangedBy,
					ChangedDateTime = @ChangedDateTime
				WHERE 	HeartbeatId = @HeartbeatId
		END
	ELSE
		BEGIN
			INSERT Msg.Heartbeat(HeartbeatId, SubscriberId, ProcessId, ChangedBy, ChangedDateTime)
			VALUES (@HeartbeatId, @SubscriberId, @ProcessId, @ChangedBy, @ChangedDateTime)			
		END
END

GO

