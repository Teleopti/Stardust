/* 
Trunk initiated: 
2010-08-19 
13:54
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-08-17
--Desc: User detail table to contain user only information for persons
----------------  
CREATE TABLE [dbo].[UserDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[LastPasswordChange] [datetime] NOT NULL,
	[InvalidAttemptsSequenceStart] [datetime] NOT NULL,
	[IsLocked] [bit] NOT NULL,
	[InvalidAttempts] [int] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UserDetail]  WITH CHECK ADD  CONSTRAINT [FK_UserDetail_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[UserDetail] CHECK CONSTRAINT [FK_UserDetail_Person]
GO

----------------  
--Name: CS
--Date: 2010-08-30
--Desc: Fix, Number in SchedulePeriod has to be > 0
----------------  
update dbo.SchedulePeriod
set Number = 1
where Number < 1
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (292,'7.1.292') 
