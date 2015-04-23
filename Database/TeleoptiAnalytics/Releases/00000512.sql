CREATE NONCLUSTERED INDEX [IX_dimPerson_PersonCode] ON [mart].[dim_person]
(
            [person_code] ASC
)
INCLUDE (
            [person_id],
            [business_unit_id]
            )

GO
