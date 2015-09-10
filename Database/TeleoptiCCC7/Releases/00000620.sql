
ALTER TABLE Tenant.Tenant ADD
	AggregationConnectionString nvarchar(500) NOT NULL CONSTRAINT DF_Tenant_AggregationConnectionString DEFAULT ''