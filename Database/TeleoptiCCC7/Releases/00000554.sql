
CREATE TABLE PayrollFormat(
	[Id] [uniqueidentifier] NOT NULL,
	FormatId [uniqueidentifier] NOT NULL,
	Name nvarchar(500) NOT NULL,
	CONSTRAINT [PK_PayrollFormat]  PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)


