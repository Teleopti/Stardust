----------------  
--Name: Mingdi
--Desc: Make constraint unique for swap details to avoid redundant records. 
---------------- 

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE dbo.ShiftTradeSwapDetail
ADD CONSTRAINT [PK_ShiftTradeSwapDetails] UNIQUE (Parent, PersonFrom, PersonTo, DateFrom, DateTo)
GO