--Robin: Removing column IsDeleted as it isn't used anymore.

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		EXECUTE('DELETE FROM dbo.StateGroupActivityAlarm WHERE IsDeleted=1')
	END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		ALTER TABLE dbo.StateGroupActivityAlarm	DROP COLUMN IsDeleted
	END
	
GO

--Anders: Adding possiblity to purge old messages cause it's been asked for
if not exists (select 1 from PurgeSetting where [Key] = 'Message')
begin
	insert into PurgeSetting values ('Message', 10)
end

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
--Name: Anders F
--Date: 2012-10-17
--Desc: #21067 Error on save after copy/paste of restrictions (nhib do insert of duplicate first and then delete...)
----------------  
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PreferenceDay]') AND name = N'uq_preference_day_per_agent')
BEGIN
EXECUTE('DROP INDEX [uq_preference_day_per_agent] ON [dbo].[PreferenceDay]')
END
GO

----------------  
--Name: David Jonsson
--Date: 2013-01-08
--Desc: Bug #21786. Remove dubplicates in PersonSkill
----------------  
--remove duplicates, but keep one.
--note: We don't consider "Active" it will be random
WITH Dubplicates AS
(
	select Id, Parent,Skill,ROW_NUMBER() OVER(PARTITION BY Parent,Skill ORDER BY Id) as rownumber
	from PersonSkill
) 
DELETE ps
FROM PersonSkill ps
INNER JOIN Dubplicates d
ON ps.Id = d.Id
WHERE d.rownumber >1;
GO

--add constraint to block new duplicates
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'UC_Parent_Skill')
ALTER TABLE PersonSkill ADD CONSTRAINT UC_Parent_Skill UNIQUE (Parent,Skill)