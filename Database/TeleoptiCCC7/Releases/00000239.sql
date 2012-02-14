/* 
Trunk initiated: 
2010-04-21 
19:04
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Roger Kratch  
--Date: 2010-04-26  
--Desc: Bygggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggger om PA  
----------------  

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PersonAccount_Person]') AND parent_object_id = OBJECT_ID(N'[dbo].[PersonAccount]'))
ALTER TABLE [dbo].[PersonAccount] DROP CONSTRAINT [FK_PersonAccount_Person]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PersonAccount_TrackingAbsence]') AND parent_object_id = OBJECT_ID(N'[dbo].[PersonAccount]'))
ALTER TABLE [dbo].[PersonAccount] DROP CONSTRAINT [FK_PersonAccount_TrackingAbsence]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PersonAccountDay_PersonAccount]') AND parent_object_id = OBJECT_ID(N'[dbo].[PersonAccountDay]'))
ALTER TABLE [dbo].[PersonAccountDay] DROP CONSTRAINT [FK_PersonAccountDay_PersonAccount]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PersonAccountTime_PersonAccount]') AND parent_object_id = OBJECT_ID(N'[dbo].[PersonAccountTime]'))
ALTER TABLE [dbo].[PersonAccountTime] DROP CONSTRAINT [FK_PersonAccountTime_PersonAccount]
GO


/****** Object:  Table [dbo].[PersonAbsenceAccount]    Script Date: 04/29/2010 15:00:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PersonAbsenceAccount](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Absence] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Person] ASC,
	[Absence] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_Absence_PersonAbsenceAccount] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])
GO

ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_Absence_PersonAbsenceAccount]
GO

ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_Person_PersonAbsenceAccount] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_Person_PersonAbsenceAccount]
GO

ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsenceAccount_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_PersonAbsenceAccount_Person_CreatedBy]
GO

ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsenceAccount_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_PersonAbsenceAccount_Person_UpdatedBy]
GO



/****** Object:  Table [dbo].[Account]    Script Date: 04/29/2010 14:57:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Account](
	[Id] [uniqueidentifier] NOT NULL,
	[AccountType] [nvarchar](255) NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Extra] [bigint] NOT NULL,
	[Accrued] [bigint] NOT NULL,
	[BalanceIn] [bigint] NOT NULL,
	[LatestCalculatedBalance] [bigint] NOT NULL,
	[StartDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Account]  WITH CHECK ADD  CONSTRAINT [FK_Account_PersonAbsenceAccount] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAbsenceAccount] ([Id])
GO

ALTER TABLE [dbo].[Account] CHECK CONSTRAINT [FK_Account_PersonAbsenceAccount]
GO


----------------  
--Name: Johan R Ryding  
--Date: 2010-05-03  
--Desc: Datapump PA  
----------------

DECLARE @SuperUserId UNIQUEIDENTIFIER

--Insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'


-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, GETDATE(), GETDATE(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 


SELECT DISTINCT pa.Parent, pa.TrackingAbsence 
INTO #paa
FROM PersonAccount pa

-- Get all PersonDayAccount
INSERT INTO PersonAbsenceAccount(Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Person, Absence)
SELECT NEWID(), 1, @SuperUserId, @SuperUserId, GETDATE(), GETDATE(), pa.Parent, pa.TrackingAbsence 
FROM #paa pa

INSERT INTO Account(Id, AccountType, Parent, Extra, Accrued, BalanceIn, LatestCalculatedBalance, StartDate)
SELECT NEWID(), 'Day', PerAbAcc.Id, pad.Extra * 864000000000, pad.Accrued * 864000000000, pad.BalanceIn * 864000000000, pad.LatestCalculatedBalance * 864000000000, pa.StartDate
FROM PersonAccount pa INNER JOIN PersonAccountDay pad ON
pa.Id = pad.PersonAccount INNER JOIN #paa paa ON
pa.Parent = paa.parent
AND pa.TrackingAbsence = paa.TrackingAbsence
INNER JOIN PersonAbsenceAccount PerAbAcc on
pa.Parent = PerAbAcc.person
AND pa.TrackingAbsence = PerAbAcc.Absence


-- Get all PersonTimeAccount
INSERT INTO Account(Id, AccountType, Parent, Extra, Accrued, BalanceIn, LatestCalculatedBalance, StartDate)
SELECT NEWID(), 'Time', PerAbAcc.Id, pat.Extra, pat.Accrued, pat.BalanceIn, pat.LatestCalculatedBalance, pa.StartDate
FROM PersonAccount pa INNER JOIN PersonAccountTime pat ON
pa.Id = pat.PersonAccount INNER JOIN #paa paa ON
pa.Parent = paa.parent
AND pa.TrackingAbsence = paa.TrackingAbsence
INNER JOIN PersonAbsenceAccount PerAbAcc on
pa.Parent = PerAbAcc.person
AND pa.TrackingAbsence = PerAbAcc.Absence

GO
-- Drop old PA tables. Not yet
--DROP TABLE PersonAccountDay
--GO
--DROP TABLE PersonAccountTime
--GO
--DROP TABLE PersonAccount
--GO

  

 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (239,'7.1.239') 
