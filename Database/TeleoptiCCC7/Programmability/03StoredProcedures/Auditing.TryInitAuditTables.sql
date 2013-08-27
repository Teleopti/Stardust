IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auditing].[TryInitAuditTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE  [Auditing].[TryInitAuditTables] 
GO

CREATE PROCEDURE [Auditing].[TryInitAuditTables] 
AS
--init on first deploy
declare @count int
declare @auditOn int

select @count = count(*) from [Auditing].[Revision]
select @auditOn = IsScheduleEnabled from [Auditing].[AuditSetting] where Id=1

if (@count=0)
begin
	if (@auditOn=1)
	begin
		exec [Auditing].[InitAuditTables]
	end
end
GO
