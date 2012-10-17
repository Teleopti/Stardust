-------------
-- David J
-- #20809
-------------
update dbo.applicationFunction
set
	FunctionCode='ResReportAdherencePerDay',
	FunctionDescription='xxResReportAdherencePerDay'
where ForeignId='D1ADE4AC-284C-4925-AEDD-A193676DBD2F'

----------------  
--Name: David J
--Date: 2012-09-26
--Desc: #18771 - Allowance bug, need to truncate readModel.ScheduleProjectionReadOnly
--		Since we are adding this on Trunk.sql => Add logic to truncate only once
----------------  
declare @bugId int
set @bugId=-17881
if not exists (select * from dbo.DatabaseVersion where BuildNumber=@bugId)
begin
	DECLARE @systemVersion varchar(100)
	SELECT TOP 1 @systemVersion=systemVersion FROM dbo.DatabaseVersion order by BuildNumber desc
	SELECT @systemVersion = @systemVersion + '.1'
	TRUNCATE TABLE readModel.ScheduleProjectionReadOnly
	INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (@bugId,@systemVersion)
end

----------------  
--Name: David J
--Date: 2012-10-05
--Desc: #20953
----------------  
--This delete any wrong indexes/constrataint added to [dbo].[MainShiftActivityLayer]
DECLARE @wrongIndexName sysname
DECLARE @DynamicSQL nvarchar(4000)
DECLARE @is_unique_constraint bit

DECLARE cur CURSOR FOR
select name,CAST(is_unique_constraint as bit) from sys.indexes where object_name(object_id) ='MainShiftActivityLayer' and is_primary_key=0 and (is_unique=1 or is_unique_constraint=1)
OPEN cur;
	FETCH NEXT FROM cur INTO @wrongIndexName,@is_unique_constraint;
	WHILE @@FETCH_STATUS = 0
	BEGIN

	IF @is_unique_constraint=1
		SELECT @DynamicSQL = 'ALTER TABLE [dbo].[MainShiftActivityLayer] DROP CONSTRAINT ' + @wrongIndexName
	ELSE
		SELECT @DynamicSQL = 'DROP INDEX '+ @wrongIndexName +' ON [dbo].[MainShiftActivityLayer]'
	
	PRINT @DynamicSQL 
	EXEC sp_executesql @DynamicSQL 


FETCH NEXT FROM cur INTO @wrongIndexName,@is_unique_constraint;
END
CLOSE cur;
DEALLOCATE cur;
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

----------------  
--Name: Anders F
--Date: 2012-10-17
--Desc: #21067 Error on save after copy/paste of restrictions (nhib do insert of duplicate first and then delete...)
----------------  
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PreferenceDay]') AND name = N'uq_preference_day_per_agent')
BEGIN
EXECUTE('DROP INDEX [uq_preference_day_per_agent] ON [dbo].[PreferenceDay] WITH ( ONLINE = OFF )')
END
GO