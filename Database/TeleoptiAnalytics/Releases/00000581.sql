create table RTA.StateQueue (
	Id int NOT NULL IDENTITY(1,1),
	Model nvarchar(max)
	
	CONSTRAINT [PK_StateQueue] PRIMARY KEY CLUSTERED 
	(
		Id ASC
	)
) ON [PRIMARY]

