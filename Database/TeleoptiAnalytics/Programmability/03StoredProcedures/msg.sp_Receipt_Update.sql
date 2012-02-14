IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Receipt_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Receipt_Update]
GO




----------------------------------------------------------------------------
-- Update a single record in Receipt
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Receipt_Update]
	@ReceiptId uniqueidentifier,
	@EventId uniqueidentifier,
	@ProcessId int,
	@ChangedBy nvarchar(20),
	@ChangedDateTime datetime
AS

UPDATE	Msg.Receipt
SET	EventId = @EventId,
	ProcessId = @ProcessId,
	ChangedBy = @ChangedBy,
	ChangedDateTime = @ChangedDateTime
WHERE 	ReceiptId = @ReceiptId



GO

