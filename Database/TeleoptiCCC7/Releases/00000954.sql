----------------  
--Name: Dumpling
--Date: 2018-03-19
--Desc: Add permission for removing dayoff on the web-teams module
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

--get parent level
SELECT @ParentForeignId = '0081'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0152' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'RemoveDayOff' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxRemoveDayOff' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0)

SET NOCOUNT OFF
GO

