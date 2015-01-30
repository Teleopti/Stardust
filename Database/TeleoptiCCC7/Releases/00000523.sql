----------------  
--Name: Xinfeng Li
--Date: 2015-01-29
--Desc: Remove unused SP for bulletin board
----------------  
SET NOCOUNT ON
	
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]
GO

SET NOCOUNT OFF
GO
