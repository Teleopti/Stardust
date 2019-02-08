IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auditing].[TryInitAuditTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE  [Auditing].[TryInitAuditTables] 
GO

CREATE PROCEDURE [Auditing].[TryInitAuditTables] 
AS
declare @auditOn int

select @auditOn = IsScheduleEnabled from [Auditing].[AuditSetting] where Id=1
--Is it currently off switched off? If so let's enable it and init.
begin tran
if @auditOn=0
begin
	update [Auditing].[AuditSetting] set IsScheduleEnabled = 1 where Id=1
	exec [Auditing].[InitAuditTables]
end
else
	print 'The Schedule Audit Trail is already enabled! This execution did not change anything.'
commit tran
GO
