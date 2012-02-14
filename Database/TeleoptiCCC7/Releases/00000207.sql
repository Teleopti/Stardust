/* 
Trunk initiated: 
2010-02-09 
08:42
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Roger Kratz
--Date: 2010-02-09  
--Desc: Confidential flags on activity & absence  
----------------  
alter table Absence 
    add Confidential BIT
GO
update Absence set Confidential=0
GO
ALTER TABLE Absence ALTER Column Confidential int NOT NULL
GO

----------------  
--Name: Tamas
--Date: 2010-02-11  
--Desc: Add the following new application function> ViewConfidential
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
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 

--application functions
--Get parent level
SELECT @ParentForeignId = '0023'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0052'	
SELECT @FunctionCode = 'ViewConfidential'
SELECT @FunctionDescription = 'xxViewConfidental'
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Robin Karlsson
--Date: 2010-02-16
--Desc: Renaming column to reflect that this date now is a date only
----------------  
sp_rename 'PersonAccount.StartDateTime','StartDate','COLUMN'
GO


----------------  
--Name: Jonas
--Date: 2010-02-17
--Desc: Add column DenyReason to table PersonRequest.
----------------  
ALTER TABLE dbo.PersonRequest ADD
	DenyReason nvarchar(300) NOT NULL CONSTRAINT DF_PersonRequest_DenyReason DEFAULT ''
GO


----------------  
--Name: Johan
--Date: 2010-02-18
--Desc: Index on PersonalShift for better performance in ETL-Tool, step stg_schedule_preference
----------------  
IF EXISTS (SELECT * FROM SYS.INDEXES WHERE name = 'IX_PersonalShift_Parent')
DROP INDEX IX_PersonalShift_Parent ON dbo.PersonalShift

GO

CREATE NONCLUSTERED INDEX IX_PersonalShift_Parent 
ON dbo.PersonalShift (Parent)

GO
----------------  
--Name: Zoë
--Date: 2010-02-22
--Desc: Must have is now on PreferenceRestriction instead of preference day
----------------  
ALTER TABLE dbo.PreferenceRestriction ADD MustHave bit NULL
GO
UPDATE dbo.PreferenceRestriction
SET MustHave = pd.MustHave
FROM dbo.PreferenceDay pd
WHERE dbo.PreferenceRestriction.Id = pd.Id
GO
ALTER TABLE dbo.PreferenceRestriction ALTER Column MustHave bit NOT NULL
GO
ALTER TABLE dbo.PreferenceDay DROP COLUMN MustHave
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (207,'7.1.207') 
