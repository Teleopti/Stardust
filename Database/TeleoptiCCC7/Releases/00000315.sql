/*
Trunk initiated: 
2011-01-27 
14:22
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

/*
   den 19 januari
   User: tamasb
   Database: new column in Account -> BalanceOut
*/

ALTER TABLE dbo.Account ADD
	BalanceOut bigint NOT NULL CONSTRAINT DF_Account_BalanceOut DEFAULT 0
GO

----------------  
--Name: Robin Karlsson
--Date: 2011-01-21
--Desc: Added detailed information about the payroll export.
----------------  
CREATE TABLE [dbo].[PayrollResultDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Message] [nvarchar](1000) NOT NULL,
	[ExceptionStackTrace] [nvarchar](max) NULL,
	[ExceptionMessage] [nvarchar](2000) NULL,
	[InnerExceptionStackTrace] [nvarchar](max) NULL,
	[InnerExceptionMessage] [nvarchar](2000) NULL,
	[Timestamp] [datetime] NOT NULL,
	[DetailLevel] [int] NOT NULL
) ON [PRIMARY]

--DJ: fix naming standard
ALTER TABLE dbo.PayrollResultDetail ADD CONSTRAINT
	PK_PayrollResultDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PayrollResultDetail]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResultDetail_PayrollResult] FOREIGN KEY([Parent])
REFERENCES [dbo].[PayrollResult] ([Id])
GO

ALTER TABLE [dbo].[PayrollResultDetail] CHECK CONSTRAINT [FK_PayrollResultDetail_PayrollResult]
GO



 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (315,'7.1.315') 
