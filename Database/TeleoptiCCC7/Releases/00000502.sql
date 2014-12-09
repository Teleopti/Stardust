----------------  
--Name: Mingdi
--Date: 2014-12-08
--Desc: Add new column for table ShiftExchangeOffer to record status for its statement
---------------- 

ALTER TABLE [dbo].[ShiftExchangeOffer] ADD [Status] [int] NOT NULL

GO