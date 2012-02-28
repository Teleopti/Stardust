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


UPDATE ApplicationFunction SET ForeignId = '0E3F340F-C05D-4A98-AD23-A019607745C9'
WHERE ForeignId = '1' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '5C133E8F-DF3E-48FC-BDEF-C6586B009481'
WHERE ForeignId = '2' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '7F918C26-4044-4F6B-B0AE-7D27625D052E'
WHERE ForeignId = '3' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'
WHERE ForeignId = '4' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '61548D4F-7D2C-4865-AB76-8A4D01800F1C'
WHERE ForeignId = '6' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '720A5D88-D2B5-49E1-83EE-8D05239094BF'
WHERE ForeignId = '7' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'C232D751-AEC5-4FD7-A274-7C56B99E8DEC'
WHERE ForeignId = '8' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'AE758403-C16B-40B0-B6B2-E8F6043B6E04'
WHERE ForeignId = '9' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '8D8544E4-6B24-4C1C-8083-CBE7522DD0E0'
WHERE ForeignId = '10' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '71BDB56D-C12F-489B-8275-04873A668D90'
WHERE ForeignId = '11' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '0065AA84-FD47-4022-ABE3-DD1B54FD096C'
WHERE ForeignId = '12' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'D1ADE4AC-284C-4925-AEDD-A193676DBD2F'
WHERE ForeignId = '13' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'F7937D02-FA54-4679-AF70-D9798E1690D5'
WHERE ForeignId = '14' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '4F5DDE81-C264-4756-B1F1-F65BFE54B16B'
WHERE ForeignId = '15' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '80D31D84-68DB-45A7-977F-75C3250BB37C'
WHERE ForeignId = '16' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '132E3AF2-3557-4EA7-813E-05CD4869D5DB'
WHERE ForeignId = '17' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '63243F7F-016E-41D1-9432-0787D26F9ED5'
WHERE ForeignId = '18' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '009BCDD2-3561-4B59-A719-142CD9216727'
WHERE ForeignId = '19' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '35649814-4DE8-4CB3-A51C-DDBA2A073E09'
WHERE ForeignId = '20' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'BAA446C2-C060-4F39-83EA-B836B1669331'
WHERE ForeignId = '21' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'D45A8874-57E1-4EB9-826D-E216A4CBC45B'
WHERE ForeignId = '22' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'EB977F5B-86C6-4D98-BEDF-B79DC562987B'
WHERE ForeignId = '23' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '479809D8-4DAE-4852-BF67-C98C3744918D'
WHERE ForeignId = '24' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = 'E15400E7-892A-4EDE-9377-AE693AA56829'
WHERE ForeignId = '25' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '2F222F0A-4571-4462-8FBE-0C747035994A'
WHERE ForeignId = '26' AND ForeignSource = 'Matrix'
UPDATE ApplicationFunction SET ForeignId = '8DE1AB0F-32C2-4619-A2B2-97385BE4C49C'
WHERE ForeignId = '27' AND ForeignSource = 'Matrix'

-- =============================================
-- Author:		Ola
-- Create date: 2012-02-28
-- Description:	New LicenseStatus table
-- =============================================
CREATE TABLE [dbo].[LicenseStatus](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[XmlString] [nvarchar](4000) NOT NULL,
	CONSTRAINT PK_LicenseStatus PRIMARY KEY CLUSTERED (Id))
GO

ALTER TABLE [dbo].[LicenseStatus]  WITH CHECK ADD  CONSTRAINT [FK_LicenseStatus_Person1] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[LicenseStatus] CHECK CONSTRAINT [FK_LicenseStatus_Person1]
GO

ALTER TABLE [dbo].[LicenseStatus]  WITH CHECK ADD  CONSTRAINT [FK_LicenseStatus_Person2] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[LicenseStatus] CHECK CONSTRAINT [FK_LicenseStatus_Person2]
GO


