IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Receipt_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Receipt_Select_All]
GO




----------------------------------------------------------------------------
-- Select a single record from Receipt
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Receipt_Select_All]

AS

SELECT	ReceiptId,
		EventId,
		ProcessId,
		ChangedBy,
		ChangedDateTime
FROM	Msg.Receipt



GO

