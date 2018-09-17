----------------  
--Name: Robin
--Date: 2018-09-17
--Desc: Correcting a spelling mistake
----------------  
SET NOCOUNT ON
	
UPDATE ApplicationFunction SET FunctionCode = 'Gamification' WHERE FunctionCode = 'Gamificaiton' --Name of the function > hardcoded

SET NOCOUNT OFF
GO

