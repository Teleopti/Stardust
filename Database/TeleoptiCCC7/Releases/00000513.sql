----------------  
--Name: Xinfeng Li, Jianguang Fang, Yanyi Wan
--Date: 2015-1-14
--Desc: Add new indicator for ShiftExchangeOffer
---------------- 
Alter Table [dbo].[ShiftExchangeOffer] Add [WishShiftType] integer null
GO

Update [dbo].[ShiftExchangeOffer] Set [WishShiftType] = 0
GO

Alter Table [dbo].[ShiftExchangeOffer] Alter Column [WishShiftType] integer not null
GO
