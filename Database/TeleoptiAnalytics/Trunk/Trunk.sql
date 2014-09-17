----------------  
--Name: Karin Jeppsson
--Date: 2014-09-17
--Desc: Bug #30395 Speed up load of dim_person to prevent deadlocks
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dimPerson_business_unit_code')
CREATE NONCLUSTERED INDEX [IX_dimPerson_business_unit_code] ON [mart].[dim_person] 
(
                             [business_unit_code] ASC
)
GO
