----------------  
-- This is duplicate with 00000523.sql to fix the error when I created that release.
-- When I add 00000523.sql, I forgot remove script file for the SP, then the SP will be recreated by DBManager.
-- So I have to remove it again to make the schema is exact same for all customers.
-- By Xinfeng Li
----------------  
SET NOCOUNT ON
	
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]
GO

SET NOCOUNT OFF
GO
