--Declare
DECLARE @now		AS datetime
DECLARE @adminId	AS uniqueidentifier
DECLARE @BuId		AS uniqueidentifier
DECLARE @skillId	AS uniqueidentifier
DECLARE @skillName	AS nvarchar(50)
DECLARE @adminLogon	AS nvarchar(50)
DECLARE @adminPwd	AS nvarchar(50)
DECLARE @roleId		AS uniqueidentifier
DECLARE @WinUser	AS nvarchar(50)
DECLARE @WinDomain	AS nvarchar(50)

--Init
SET @now	= getdate()
SET @skillId	= newid()
SET @skillName	= N'Default'
SET @adminId	= newid()
SET @adminLogon	= N'xxxADMIN_USER'
SET @adminPwd	= N'xxxADMIN_PWD'
SET @WinUser	= N'xxxUSERNAME'
SET @WinDomain	= N'xxxUSERDOMAIN'
SELECT @BuId	= id FROM dbo.BusinessUnit		WHERE Name = 'xxxBusiness_unit'
SET @roleId	= '193AD35C-7735-44D7-AC0C-B8EDA0011E5F' --_SuperRole

--Nhib-style
DECLARE @p0 int,@p1 uniqueidentifier,@p2 datetime,@p3 nvarchar(4000),@p4 nvarchar(4000),@p5 nvarchar(4000),@p6 datetime,@p7 nvarchar(12),@p8 nvarchar(20),@p9 uniqueidentifier,@p10 nvarchar(4000),@p11 int,@p12 int,@p13 nvarchar(4000),@p14 nvarchar(4000),@p15 nvarchar(4000),@p16 nvarchar(4000),@p17 bit,@p18 uniqueidentifier,@p19 uniqueidentifier,@p20 bit
SET @p0=1
SET @p1=@adminId
SET @p2=@now
SET @p3=N''
SET @p4=N''
SET @p5=N''
SET @p6=NULL
SET @p7=N'Admin'
SET @p8=N'Administrator'
SET @p9=NULL
SET @p10=N'UTC'
SET @p11=2057
SET @p12=2057
SELECT @p13=RTRIM(LTRIM(@WinUser))
SELECT @p14=RTRIM(LTRIM(@WinDomain))
SET @p15=@adminLogon
SET @p16=@adminPwd
SET @p17=0
SET @p18=@adminId  --Admin CreatedBy Admin
SET @p19 = @roleId
SET @p20 = 0 --Can be change by user

--Check we have don't already have a Admin User
IF NOT EXISTS (SELECT id FROM person WHERE ApplicationLogOnName = @adminLogon)
BEGIN

	INSERT INTO dbo.Person
	(Version, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn, Email, Note, EmploymentNumber, TerminalDate, FirstName, LastName, PartOfUnique, DefaultTimeZone, Culture, UiCulture, WindowsLogOnName, DomainName, ApplicationLogOnName, Password, IsDeleted, Id, BuiltIn)
	VALUES (@p0, @p1, @p2, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18,@p20)
	IF @@ERROR <> 0 PRINT 'Error inserting Admin user'
	ELSE PRINT 'Inserted Admin user'

	INSERT INTO dbo.PersonInApplicationRole (Person, ApplicationRole)
	VALUES (@p18, @p19)
	IF @@ERROR <> 0 PRINT 'Error connecting person and application role'
	ELSE PRINT 'Inserted relation in PersonInApplicationRole'

END

--Check we have don't already have a Default skill AND
--that that the DefaultBusinessUnit exists

IF	NOT EXISTS	(SELECT id FROM dbo.Scenario		WHERE Name	= @skillName)	AND
	EXISTS		(SELECT id FROM dbo.BusinessUnit	WHERE id	= @BuId)
BEGIN
INSERT INTO [dbo].[Scenario]
           ([Id]
           ,[Version]
           ,[CreatedBy]
           ,[UpdatedBy]
           ,[CreatedOn]
           ,[UpdatedOn]
           ,[Name]
           ,[ShortName]
           ,[DefaultScenario]
           ,[Audittrail]
           ,[BusinessUnit]
           ,[IsDeleted]
           ,[EnableReporting])
     VALUES
           (@skillId,
           1,
           @adminId,
           @adminId,
           @now,
           @now,
           @skillName,
           @skillName,
			1,
			1,
			@BuId,
			0,
			1)

	IF @@ERROR <> 0 PRINT 'Error inserting default scenario'
	ELSE PRINT 'Inserted default scenario'
END