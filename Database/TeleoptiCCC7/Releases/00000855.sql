------------------------------------------------------------------------------------------------
--Name: Xinfeng
--Date: 2017-06-19
--Desc: Make sure permission ShiftTradeBulletinBoard is in same level of ShiftTradeRequests
------------------------------------------------------------------------------------------------

DECLARE @MyTimeWebPermissionId as uniqueidentifier
DECLARE @ShiftTradeBulletinBoardPermissionId as uniqueidentifier
DECLARE @ShiftTradeBulletinBoardParentId as uniqueidentifier

SELECT @MyTimeWebPermissionId=Id
  FROM ApplicationFunction
 WHERE FunctionCode = 'MyTimeWeb' AND FunctionDescription = 'xxMyTimeWeb'

SELECT @ShiftTradeBulletinBoardPermissionId = Id, @ShiftTradeBulletinBoardParentId = Parent
  FROM ApplicationFunction
 WHERE FunctionCode = 'ShiftTradeBulletinBoard' AND FunctionDescription = 'xxShiftTradeBulletinBoard'

IF @ShiftTradeBulletinBoardParentId <> @MyTimeWebPermissionId
BEGIN
   UPDATE ApplicationFunction
      SET Parent = @MyTimeWebPermissionId
    WHERE Id = @ShiftTradeBulletinBoardPermissionId
END
