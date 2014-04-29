--Name: Ola, Jonas, Kunning
--Date: 2014-04-29
--Desc: Add the authentication info table to prepare for single sign on
-- SHOULD DELETE WindowsAuthenticationInfo table later on.
----------------  

CREATE TABLE [dbo].[AuthenticationInfo](
	[Person] [uniqueidentifier] NOT NULL,
	[Identity] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_AuthenticationInfo] PRIMARY KEY NONCLUSTERED 
(
	[Person] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Identity] UNIQUE CLUSTERED 
(
	[Identity] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AuthenticationInfo] WITH CHECK ADD CONSTRAINT [FK_AuthenticationInfo_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[AuthenticationInfo] CHECK CONSTRAINT [FK_AuthenticationInfo_Person]
GO

INSERT INTO [AuthenticationInfo] 
SELECT Person, DomainName+'\'+WindowsLogOnName
FROM WindowsAuthenticationInfo

GO

DROP TABLE WindowsAuthenticationInfo