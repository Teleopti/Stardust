----------------  
--Name: David J + Andreas S
--Date: 2012-00-11
--Desc: prepare Table for PS Tech custom SMS bridge
----------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CustomTables].[SMS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [CustomTables].[SMS](
		[Id] [uniqueidentifier] NOT NULL,
		[PhoneNumber] [nvarchar](50) NULL,
		[Msg] nvarchar(max) NULL,
		[RecivedTime] [datetime] NULL,
		[DeliveredTime] [datetime] NULL,
		[LastTriedTime] [datetime] NULL,
		[IsSent] [bit] NOT NULL DEFAULT 0,
		[ToBeSent] [bit] NOT NULL DEFAULT 1,
		[Status] [nvarchar](200) NOT NULL DEFAULT 'init'
		)
	           
	ALTER TABLE CustomTables.SMS ADD CONSTRAINT
		PK_SMS PRIMARY KEY CLUSTERED 
		(
		Id
		)
END
GO
