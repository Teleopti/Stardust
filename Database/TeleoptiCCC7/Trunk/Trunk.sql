--#21539
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Team_Id_Parent')
CREATE NONCLUSTERED INDEX IX_PersonPeriod_Team_Id_Parent
ON [dbo].[PersonPeriod] ([Team])
INCLUDE ([Id],[Parent])
GO

-----------------  
--Name: TamasB
--Date: 2012-12-17
--Desc: #bugfix 21764 - Fix invalid text
----------------  
UPDATE [dbo].PersonRequest
SET DenyReason = 'RequestDenyReasonSupervisor'
WHERE DenyReason = 'xxRequestDenyReasonSupervisor'
GO