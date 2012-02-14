IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Receipts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Receipts]
GO



CREATE PROCEDURE [Msg].[sp_Receipts]
AS
SELECT ReceiptId, EventId, ProcessId, ChangedBy, ChangedDateTime 
FROM Msg.receipt 
ORDER BY changeddatetime DESC

GO

