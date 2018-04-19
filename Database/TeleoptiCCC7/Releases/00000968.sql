-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-19
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

DROP INDEX CIX_PersonGroup_PersonGroup ON dbo.PersonGroup
GO
ALTER TABLE dbo.PersonGroup ADD CONSTRAINT
	PK_PersonGroup PRIMARY KEY CLUSTERED 
	(
		PersonGroup,
		Person
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

-----------------------------------------------------------

DECLARE @DROPINDEXQUERY nvarchar(MAX)
DECLARE @INDEXNAME nvarchar(MAX)

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[PersonInApplicationRole]') 
	AND type in (N'UQ')
	and name like 'UQ%[_][_]%%' -- Look for UQ__XXXXXXXXXX__XXXXXXX
	)

select @INDEXNAME

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.PersonInApplicationRole drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end
GO

ALTER TABLE dbo.PersonInApplicationRole ADD CONSTRAINT
	PK_PersonInApplicationRole PRIMARY KEY CLUSTERED 
	(
		Person,
		ApplicationRole
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


