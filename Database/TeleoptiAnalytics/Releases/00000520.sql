--already addded with one customer
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_personPeriodCode_IncludePersonId_businessUnitId')
CREATE NONCLUSTERED INDEX [IX_dim_person_personPeriodCode_IncludePersonId_businessUnitId] ON [mart].[dim_person]
(
	[person_period_code] ASC
)
INCLUDE ([person_id],business_unit_id)
GO