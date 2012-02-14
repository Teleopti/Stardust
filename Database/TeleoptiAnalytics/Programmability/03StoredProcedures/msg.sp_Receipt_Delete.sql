IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Receipt_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Receipt_Delete]
GO




----------------------------------------------------------------------------
-- Delete a single record from Receipt
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Receipt_Delete]
	@ReceiptId uniqueidentifier
AS

DELETE	Msg.Receipt
WHERE 	ReceiptId = @ReceiptId



GO

