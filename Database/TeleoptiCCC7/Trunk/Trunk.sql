----------------  
--Name: Anders Forsberg
--Date: 2012-10-15
--Desc: Adding possiblity to purge old payroll exports cause it's been asked for
----------------  
if not exists (select 1 from PurgeSetting where [Key] = 'Payroll')
begin
	insert into PurgeSetting values ('Payroll', 20)
end
GO
----------------  
--Name: Xianwei Shen
--Date: 2012-10-15
--Desc: #21058 Purge OptionalColumnValue when its parent is deleted
----------------  
IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.OptionalColumn','U') AND name='IsDeleted')
BEGIN
EXECUTE(
'DELETE FROM dbo.OptionalColumnValue 
WHERE Parent IN (
   SELECT Id FROM dbo.OptionalColumn WHERE IsDeleted = 1
)
DELETE FROM dbo.OptionalColumn WHERE IsDeleted = 1
ALTER TABLE dbo.OptionalColumn
           DROP COLUMN IsDeleted
ALTER TABLE dbo.OptionalColumnValue
           DROP CONSTRAINT FK_OptionalColumnValue_OptionalColumn
ALTER TABLE dbo.OptionalColumnValue ADD CONSTRAINT
          FK_OptionalColumnValue_OptionalColumn FOREIGN KEY(Parent) 
           REFERENCES dbo.OptionalColumn(Id)
          ON DELETE  CASCADE')
END
GO

