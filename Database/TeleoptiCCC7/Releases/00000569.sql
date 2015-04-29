----------------  
--Name: Micke
--Date: 2015-04-30
--Desc: Adding the new skill type Outbound to the database.
----------------  
DECLARE @creator uniqueidentifier
SELECT TOP 1 @creator=UpdatedBy FROM SkillType
IF @creator IS NULL
	BEGIN
		SELECT TOP 1 @creator=Id FROM Person
	END
INSERT INTO SkillType (Id,ForecastType,Version,UpdatedBy,UpdatedOn,Name,ShortName,ForecastSource,IsDeleted)
VALUES (NEWID(),0,1,@creator,GETUTCDATE(),N'SkillTypeOutbound',null,4,0)