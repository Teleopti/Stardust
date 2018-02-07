----------------  
--Name: Anders Forsberg
--Date: 2012-10-15
--Desc: Adding possiblity to purge old payroll exports cause it's been asked for
----------------  

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

----------------  
--Name: Anders F
--Date: 2012-10-17
--Desc: #21067 Error on save after copy/paste of restrictions (nhib do insert of duplicate first and then delete...)
----------------  
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PreferenceDay]') AND name = N'uq_preference_day_per_agent')
BEGIN
EXECUTE('DROP INDEX [uq_preference_day_per_agent] ON [dbo].[PreferenceDay]')
END
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (374,'7.3.374') 
