-----------------  
---Name: Jonas
---Date: 2012-11-30  
---Desc: Bug #21668 Change column for day off ShortName to nullable in stage.stg_schedule_preference table.
-----------------  
TRUNCATE TABLE stage.stg_schedule_preference
ALTER TABLE stage.stg_schedule_preference ALTER COLUMN day_off_shortname NVARCHAR(25) NULL

----------------  
--Name: David
--Date: 2012-12-03
--Desc: Bug #21676 - Too many agents. Found some possible performance issues as well. adding a few indexes on dim x 2
----------------  	
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND name = N'IX_dim_acdLogin_datasourceId')
DROP INDEX [IX_dim_acdLogin_datasourceId] ON [mart].[dim_acd_login]
GO
CREATE NONCLUSTERED INDEX IX_dim_acdLogin_datasourceId
ON [mart].[dim_acd_login] ([datasource_id])
INCLUDE ([acd_login_id],[acd_login_original_id])
GO
---
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dimPerson_personCode_PersonBuInclude')
DROP INDEX [IX_dimPerson_personCode_PersonBuInclude] ON [mart].[dim_person]
GO
CREATE NONCLUSTERED INDEX IX_dimPerson_personCode_PersonBuInclude
ON [mart].[dim_person] ([person_code])
INCLUDE ([person_id],[business_unit_code])
---
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dimPerson_personCode_DateIncludes')
DROP INDEX [IX_dimPerson_personCode_DateIncludes] ON [mart].[dim_person]
GO
CREATE NONCLUSTERED INDEX IX_dimPerson_personCode_DateIncludes
ON [mart].[dim_person] ([person_code])
INCLUDE ([person_id],[valid_from_date_id],[valid_to_date_id],[business_unit_code])
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (376,'7.3.376') 
