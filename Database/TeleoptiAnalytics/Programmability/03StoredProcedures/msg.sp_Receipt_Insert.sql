IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Receipt_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Receipt_Insert]
GO




----------------------------------------------------------------------------
-- Insert a single record into Receipt
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Receipt_Insert]
	@ReceiptId uniqueidentifier OUTPUT,
	@EventId uniqueidentifier,
	@ProcessId int,
	@ChangedBy nvarchar(10),
	@ChangedDateTime datetime
AS
BEGIN
IF(@ReceiptId = '00000000-0000-0000-0000-000000000000')
	BEGIN
		SET @ReceiptId = newid()
		INSERT Msg.Receipt(ReceiptId, EventId, ProcessId, ChangedBy, ChangedDateTime)
		VALUES (@ReceiptId, @EventId, @ProcessId, @ChangedBy, @ChangedDateTime)
	END
ELSE
	IF(EXISTS(SELECT 1 FROM Msg.Receipt WHERE ReceiptId = @ReceiptId))
		BEGIN
			UPDATE	Msg.Receipt
			SET	EventId = @EventId,
				ProcessId = @ProcessId,
				ChangedBy = @ChangedBy,
				ChangedDateTime = @ChangedDateTime
			WHERE 	ReceiptId = @ReceiptId
		END
	ELSE
		BEGIN
			INSERT Msg.Receipt(ReceiptId, EventId, ProcessId, ChangedBy, ChangedDateTime)
			VALUES (@ReceiptId, @EventId, @ProcessId, @ChangedBy, @ChangedDateTime)
		END
END




GO

