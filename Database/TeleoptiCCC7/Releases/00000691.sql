----------------  
--Name: Junwen
--Date: 2016-06-15
--Desc: Separate permission ShiftTradeBulletinBoard from permission ShiftTradeRequests
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @ShiftTradeBulletinBoardPermissionId as uniqueidentifier
DECLARE @MyTimeWebPermissionId as uniqueidentifier

select @MyTimeWebPermissionId = Parent from ApplicationFunction where FunctionCode = 'ShiftTradeRequests' and FunctionDescription = 'xxShiftTradeRequests'
select @ShiftTradeBulletinBoardPermissionId = Id from ApplicationFunction where FunctionCode = 'ShiftTradeBulletinBoard'

update ApplicationFunction set Parent = @MyTimeWebPermissionId where Id = @ShiftTradeBulletinBoardPermissionId

SET NOCOUNT OFF
GO

