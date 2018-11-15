CREATE TABLE [Auditing].[StaffingAudit](
            [Id] [uniqueidentifier] NOT NULL,
            [TimeStamp] [datetime] NOT NULL,
            [ActionPerformedBy] [uniqueidentifier] NOT NULL,
            [Action] [nvarchar](255) NOT NULL,
            [Area] [nvarchar](255) NOT NULL,
            [ImportFileName] [nvarchar](500),
            [BpoId] [uniqueidentifier],
            [ClearPeriodStart] [datetime],
            [ClearPeriodEnd] [datetime],
CONSTRAINT [PK_StaffingAudit] PRIMARY KEY CLUSTERED 
(
            [Id] ASC
)
)
GO

ALTER TABLE [Auditing].[StaffingAudit]  WITH CHECK ADD  CONSTRAINT [FK__PER_SA_ActionPerformedBy_Person_Id] FOREIGN KEY([ActionPerformedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [Auditing].[StaffingAudit] CHECK CONSTRAINT [FK__PER_SA_ActionPerformedBy_Person_Id]
GO


SET NOCOUNT ON
            
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

SELECT      @SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

--get parent level
SELECT @ParentForeignId = '0006'     --Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
            
--insert/modify application function
SELECT @ForeignId = '0060' --Foreign id of the function > hardcoded        
SELECT @FunctionCode = 'GeneralAuditTrailWebReport' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxGeneralAuditTrailWebReport' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0)

SET NOCOUNT OFF
GO

if not exists (select 1 from PurgeSetting where [key] = 'MonthsToKeepAudit')
	insert into PurgeSetting ([Key], [Value]) values ('MonthsToKeepAudit', 3)