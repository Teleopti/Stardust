CREATE TABLE Tenant.ServerConfiguration
(
	[Key] nvarchar(255) NOT NULL,
	[Value] nvarchar(max)
	
	CONSTRAINT [PK_ServerConfiguration] PRIMARY KEY CLUSTERED ([Key])
)

INSERT INTO Tenant.ServerConfiguration values('FrameAncestors', '') 

GO
