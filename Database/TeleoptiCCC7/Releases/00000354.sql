/*
Anders F
2012-02-06
Purge old data for app db.
*/

IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurgeSetting]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[PurgeSetting](
		[Key] [nvarchar](50) NOT NULL,
		[KeepYears] [int] NOT NULL
		)


	ALTER TABLE [dbo].[PurgeSetting] ADD CONSTRAINT PK_PurgeSetting PRIMARY KEY CLUSTERED 
	(
		[Key] ASC
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[PurgeSetting] WHERE [Key] ='Forecast')
	INSERT INTO [dbo].[PurgeSetting] ([Key], [KeepYears]) VALUES('Forecast',10)
IF NOT EXISTS (SELECT 1 FROM [dbo].[PurgeSetting] WHERE [Key] ='Schedule')
	INSERT INTO [dbo].[PurgeSetting] ([Key], [KeepYears]) VALUES('Schedule',10)
GO
----------------  
--Name: Peter W
--Date: 2012-02-02
--Desc: Add the following new application function AnyTime (to under construction)
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

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1, 1) 

--get parent level
SELECT @ParentForeignId = '0048'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0074' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'Anywhere' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxAnywhere' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO


-- =============================================
-- Author:		David
-- Create date: 2012-02-08
-- Description:	implemented with view instead
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[CollationCulture]') AND type in (N'U'))
DROP TABLE [ReadModel].[CollationCulture]
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (354,'7.1.354') 
