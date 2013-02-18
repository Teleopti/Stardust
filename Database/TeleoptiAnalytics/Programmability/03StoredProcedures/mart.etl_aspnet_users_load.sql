IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_aspnet_users_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_aspnet_users_load]
GO


-- =============================================
-- Author:		JoNo
-- Create date: 2008-11-05
-- Update date: 2009-02-11 New schemas for tables KJ
--				2010-10-05 Added removal of users not begin users anymore RK
-- Description:	Also loads user/login info from stg_user to aspnet_Users and aspnet_Membership
-- =============================================
CREATE PROCEDURE [mart].[etl_aspnet_users_load] 
	
AS

DECLARE @application_id uniqueidentifier
SELECT TOP 1 @application_id = ApplicationId 
FROM dbo.aspnet_applications

---------------------------------------------------------------------------
-- update changes on users
UPDATE dbo.aspnet_Users
SET 
	AppLoginName	= lower(su.application_logon_name),
	UserName		= su.windows_domain_name + '\' + su.windows_logon_name,
	LoweredUserName	= lower(su.windows_domain_name + '\' + su.windows_logon_name),
	FirstName		= su.person_first_name,
	LastName		= su.person_last_name,
	LanguageId		= su.language_id,
	CultureId		= su.culture
FROM
	Stage.stg_user su
WHERE 
	su.person_code = dbo.aspnet_Users.UserId

UPDATE dbo.aspnet_Users
SET 
	UserName		= '',
	LoweredUserName	= ''
FROM
	Stage.stg_user su
WHERE 
	rtrim(UserName) = '\' OR
	ltrim(UserName) = '\'
---------------------------------------------------------------------------

-- Insert new users
INSERT INTO dbo.aspnet_Users
	(
	ApplicationId,
	UserId,
	UserName,
	LoweredUserName,
	IsAnonymous,
	LastActivityDate,
	AppLoginName,
	FirstName,
	LastName,
	LanguageId,
	CultureId
	)
SELECT
	ApplicationId		= @application_id,
	UserId				= su.person_code,
	UserName			= su.windows_domain_name + '\' + su.windows_logon_name,
	LoweredUserName		= lower(su.windows_domain_name + '\' + su.windows_logon_name),
	IsAnonymous			= 0,
	LastActivityDate	= '1900-01-01',
	AppLoginName		= lower(su.application_logon_name),
	FirstName			= su.person_first_name,
	LastName			= su.person_last_name,
	LanguageId			= su.language_id,
	CultureId			= su.culture
FROM
	Stage.stg_user su
WHERE
	NOT EXISTS (SELECT UserId FROM dbo.aspnet_Users au WHERE au.UserId = su.person_code AND au.ApplicationId = @application_id)

---------------------------------------------------------------------------

-- update user membership
UPDATE dbo.aspnet_membership
SET 
	Password		= su.password,
	PasswordSalt	= su.password,
	Email			= su.email,
	LoweredEmail	= lower(su.email)
FROM
	Stage.stg_user su
INNER JOIN
	dbo.aspnet_Users au
ON
	su.person_code = au.UserId
INNER JOIN
	dbo.aspnet_membership m
ON 
	au.UserId = m.UserId
---------------------------------------------------------------------------

-- Insert user membership
INSERT INTO dbo.aspnet_Membership
	(
	ApplicationId,
	UserId,
	Password,
	PasswordFormat,
	PasswordSalt,
	Email,
	LoweredEmail,
	IsApproved,
	IsLockedOut,
	CreateDate,
	LastLoginDate,
	LastPasswordChangedDate,
	LastLockoutDate,
	FailedPasswordAttemptCount,
	FailedPasswordAttemptWindowStart,
	FailedPasswordAnswerAttemptCount,
	FailedPasswordAnswerAttemptWindowStart
	)
SELECT
	ApplicationId							= @application_id,
	UserId									= su.person_code,
	Password								= su.password,
	PasswordFormat							= 0,
	PasswordSalt							= su.password,
	Email									= su.email,
	LoweredEmail							= lower(su.email),
	IsApproved								= 1,
	IsLockedOut								= 0,
	CreateDate								= getdate(),
	LastLoginDate							= '1900-01-01',
	LastPasswordChangedDate					= '1900-01-01',
	LastLockoutDate							= '1900-01-01',
	FailedPasswordAttemptCount				= 0,
	FailedPasswordAttemptWindowStart		= '1900-01-01',
	FailedPasswordAnswerAttemptCount		= 0,
	FailedPasswordAnswerAttemptWindowStart	= '1900-01-01'
FROM
	Stage.stg_user su
INNER JOIN
	dbo.aspnet_Users au
ON
	su.person_code = au.UserId
WHERE NOT EXISTS (SELECT ApplicationId, UserId FROM dbo.aspnet_Membership m WHERE m.UserId = au.UserId AND ApplicationId = @application_id)

---------------------------------------------------------------------------
--- Remove obsolete users

DELETE dbo.aspnet_Membership FROM dbo.aspnet_Membership m LEFT JOIN Stage.stg_user su ON su.person_code=m.UserId WHERE su.person_code IS NULL

DELETE dbo.aspnet_Users FROM dbo.aspnet_Users u LEFT JOIN Stage.stg_user su ON su.person_code=u.UserId WHERE su.person_code IS NULL

GO