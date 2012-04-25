--Improve delete in: mart.etl_fact_schedule_load
--Adding on 358 and default => IF NOT EXISTS
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_Shift_startdate_id')
CREATE NONCLUSTERED INDEX [IX_fact_schedule_Shift_startdate_id]
ON [mart].[fact_schedule] ([shift_startdate_id])
INCLUDE ([schedule_date_id],[person_id],[interval_id],[activity_starttime],[scenario_id],[business_unit_id])
GO

----------------
--PBI 19157
--David J
--New hiararcy in the Cube: WindowsCredentials
----------------
/*
--revert
ALTER TABLE mart.dim_person DROP COLUMN windows_domain
ALTER TABLE mart.dim_person DROP COLUMN windows_username
ALTER TABLE stage.stg_person DROP COLUMN windows_domain
ALTER TABLE stage.stg_person DROP COLUMN windows_username
*/
--stage
ALTER TABLE stage.stg_person ADD
	windows_domain		nvarchar(50) NULL,
	windows_username	nvarchar(50) NULL

--mart	
ALTER TABLE mart.dim_person ADD
	windows_domain		nvarchar(50) NULL,
	windows_username	nvarchar(50) NULL
GO
UPDATE mart.dim_person 
SET windows_domain	 = 'Not Defined',
	windows_username = 'Not Defined'

ALTER TABLE mart.dim_person ALTER COLUMN windows_domain		nvarchar(50) NOT NULL
ALTER TABLE mart.dim_person ALTER COLUMN windows_username	nvarchar(50) NOT NULL
GO
