--Name: Ola, Jonas, Kunning
--Date: 2014-04-29
--Desc: Add the authentication info table to prepare for single sign on
----------------  
IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuthenticationInfo]') AND type in (N'U'))
BEGIN
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

ALTER TABLE [dbo].[AuthenticationInfo] WITH CHECK ADD CONSTRAINT [FK_AuthenticationInfo_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[AuthenticationInfo] CHECK CONSTRAINT [FK_AuthenticationInfo_Person]

INSERT INTO [AuthenticationInfo] 
SELECT Person, DomainName+'\'+WindowsLogOnName
FROM WindowsAuthenticationInfo

END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WindowsAuthenticationInfo]') AND type in (N'U'))
DROP TABLE [dbo].[WindowsAuthenticationInfo]
GO
