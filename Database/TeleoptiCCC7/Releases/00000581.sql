UPDATE dbo.ShiftTradeRequest
SET offer=NULL
FROM dbo.ShiftTradeRequest
WHERE offer NOT IN (SELECT request FROM  dbo.ShiftExchangeOffer) AND offer IS NOT NULL
GO
ALTER TABLE [dbo].[ShiftTradeRequest]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeRequest_ShiftExchangeOffer] FOREIGN KEY([Offer])
REFERENCES [dbo].[ShiftExchangeOffer] ([Request])
GO

ALTER TABLE [dbo].[ShiftTradeRequest] CHECK CONSTRAINT [FK_ShiftTradeRequest_ShiftExchangeOffer]
GO


