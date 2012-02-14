IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Receipt_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Receipt_Select]
GO




----------------------------------------------------------------------------
-- Select a single record from Receipt
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Receipt_Select]
	@ReceiptId uniqueidentifier
AS

SELECT	ReceiptId,
	EventId,
	ProcessId,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Receipt
WHERE 	ReceiptId = @ReceiptId


GO

