----------------  
--Name: ChatBot
--Date: 2018-09-11
--Desc: Add new application function "ChatBot"
----------------  
SET NOCOUNT ON

DECLARE @ParentId as uniqueidentifier
DECLARE @ParentForeignId as varchar(255)

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
UPDATE ApplicationFunction SET Parent=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId ='0161'

SET NOCOUNT OFF
GO

