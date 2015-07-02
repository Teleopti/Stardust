----------------  
--Name: Mingdi
--Desc: Make constraint unique for swap details to avoid redundant records. 
---------------- 

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


DELETE dbo.ShiftTradeSwapDetail 
FROM dbo.ShiftTradeSwapDetail
LEFT OUTER JOIN (
   SELECT CONVERT(uniqueidentifier, MIN(CONVERT(char(36), id))) as RowId, Parent, PersonFrom, PersonTo, DateFrom, DateTo
   FROM dbo.ShiftTradeSwapDetail 
   GROUP BY Parent, PersonFrom, PersonTo, DateFrom, DateTo
) as KeepRows ON
   dbo.ShiftTradeSwapDetail.Id = KeepRows.RowId
WHERE
   KeepRows.RowId IS NULL

ALTER TABLE dbo.ShiftTradeSwapDetail
ADD CONSTRAINT [PK_ShiftTradeSwapDetails] UNIQUE (Parent, PersonFrom, PersonTo, DateFrom, DateTo)
GO