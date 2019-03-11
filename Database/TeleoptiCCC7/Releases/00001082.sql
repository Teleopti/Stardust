CREATE SCHEMA [status] AUTHORIZATION [dbo]
GO

CREATE TABLE status.CustomStatusStep(
	Id int not null identity(1,1),
	[Name] nvarchar(255) not null,
	Description nvarchar(1000) not null,
	SecondsLimit int not null,
	LastPing DateTime2 not null,
	CONSTRAINT [PK_CustomStatusStep] PRIMARY KEY CLUSTERED 
	(
		Id ASC
	)
)
GO

insert into status.CustomStatusStep (name, description, secondslimit, lastping) values
('ETL', 'Verifies that ETL service is alive.', 40, '2015-1-1')
