
/****** Object:  StoredProcedure [ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]    Script Date: 2013-11-14 13:35:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]
GO

