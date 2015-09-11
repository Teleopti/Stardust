CREATE TABLE Tenant.UpgradeLog
	(
	Id int NOT NULL IDENTITY (1, 1),
	Tenant nvarchar(500) NOT NULL,
	Time datetime NOT NULL,
	[Level] nvarchar(100) NOT NULL,
	Message nvarchar(MAX) NOT NULL
	) 

