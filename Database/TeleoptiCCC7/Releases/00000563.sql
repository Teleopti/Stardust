
CREATE TABLE [Tenant].[PasswordPolicyForUser](
	[Id] [uniqueidentifier] NOT NULL,
	[LastPasswordChange] [datetime] NOT NULL,
	[InvalidAttemptsSequenceStart] [datetime] NOT NULL,
	[IsLocked] [bit] NOT NULL,
	[InvalidAttempts] [int] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL)

	USE [main_clone_DemoSales_TeleoptiCCC7]
GO

ALTER TABLE  [Tenant].[PasswordPolicyForUser] ADD  CONSTRAINT [PK_PasswordPolicyForUser] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
GO

ALTER TABLE Tenant.PasswordPolicyForUser ADD CONSTRAINT
	FK_PasswordPolicyForUser_PersonInfo FOREIGN KEY
	(
	Person
	) REFERENCES Tenant.PersonInfo
	(
	Id
	) 
--GO