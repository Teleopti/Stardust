CREATE NONCLUSTERED INDEX [IX_dim_person_personPeriodCode_IncludePersonId_businessUnitId] ON [mart].[dim_person]
(
	[person_period_code] ASC
)
INCLUDE ([person_id],business_unit_id)
GO