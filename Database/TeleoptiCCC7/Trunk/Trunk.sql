
-----------------  
--Name: TamasB
--Date: 2012-12-17
--Desc: #bugfix 21764 - Fix invalid text
----------------  
UPDATE [dbo].PersonRequest
SET DenyReason = 'RequestDenyReasonSupervisor'
WHERE DenyReason = 'xxRequestDenyReasonSupervisor'
GO