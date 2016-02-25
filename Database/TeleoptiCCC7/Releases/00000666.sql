----------------  
--Name: Xinfeng
--Date: 2016-02-25
--Desc: Add new application functions for MyTeam in Angel
----------------  
SET NOCOUNT ON

--declarations
DECLARE @SuperUserId AS UNIQUEIDENTIFIER
DECLARE @FunctionId AS UNIQUEIDENTIFIER
DECLARE @ParentFunctionId AS UNIQUEIDENTIFIER
DECLARE @ForeignId AS VARCHAR(255)
DECLARE @ParentForeignId AS VARCHAR(255)
DECLARE @FunctionCode AS VARCHAR(255)
DECLARE @FunctionDescription AS VARCHAR(255)
DECLARE @ParentId AS UNIQUEIDENTIFIER

--insert to super user if not exist
SELECT @SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- get parent level
SELECT @ParentForeignId = '0001' -- Put this item in root level

SELECT @ParentId = Id
  FROM ApplicationFunction
 WHERE ForeignSource = 'Raptor'
   AND IsDeleted = 'False'
   AND ForeignId LIKE (@ParentForeignId + '%')

--insert/modify application function
SELECT @ForeignId = '0129' --Foreign id of the function > hardcoded    
SELECT @FunctionCode = 'AngelMyTeamSchedules' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxMyTeam' --Description of the function > hardcoded

IF (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource = 'Raptor' AND IsDeleted = 'False' AND ForeignId LIKE (@ForeignId + '%')))
   INSERT [dbo].[ApplicationFunction] ( [Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId],
          [ForeignSource], [IsDeleted])
   VALUES (newid(), 1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0)

SET NOCOUNT OFF
GO
