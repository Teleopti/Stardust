DECLARE @skillgroupFuncExists BIT, @functionCodeSkillGroup VARCHAR(MAX), @functionCodeSkillArea VARCHAR(MAX)

SET @functionCodeSkillGroup = 'WebModifySkillGroup'
SET @functionCodeSkillArea = 'WebModifySkillArea'

SELECT @skillgroupFuncExists = CASE COUNT(*) WHEN 1 THEN 1 ELSE 0 END FROM [dbo].[ApplicationFunction] WHERE FunctionCode = @functionCodeSkillGroup

IF @skillgroupFuncExists = 1 
BEGIN
  UPDATE [dbo].[ApplicationFunction] SET IsDeleted = 1 WHERE FunctionCode = @functionCodeSkillArea;
  UPDATE [dbo].[ApplicationFunctionInRole] SET ApplicationFunction = (SELECT TOP 1 id FROM [dbo].[ApplicationFunction] WHERE FunctionCode = @functionCodeSkillGroup)
  WHERE ApplicationFunction = (SELECT TOP 1 id FROM [dbo].[ApplicationFunction] WHERE FunctionCode = @functionCodeSkillArea)
END
 
UPDATE [dbo].[ApplicationFunction] 
SET Parent = (SELECT TOP 1 id FROM [dbo].[ApplicationFunction] WHERE FunctionCode = 'Anywhere'),
FunctionCode = @functionCodeSkillGroup,
FunctionDescription = 'xxModifySkillGroup' 
WHERE (FunctionCode = @functionCodeSkillArea OR FunctionCode = @functionCodeSkillGroup) AND IsDeleted = 0